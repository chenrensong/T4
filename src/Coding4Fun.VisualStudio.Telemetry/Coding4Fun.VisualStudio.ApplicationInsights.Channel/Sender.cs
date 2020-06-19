using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using Coding4Fun.VisualStudio.LocalLogger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Fetch transmissions from the storage and sends it.
	/// </summary>
	internal class Sender : IDisposable
	{
		/// <summary>
		/// We need to keep number of stored transmission hashes limited. So once per
		/// TriggerCountStaleHashCheck sent transmission we start cleanup procedure and
		/// remove stale hashes which is in memory for more than StaleHashPeriod of time.
		/// Each hash is 64 characters long, so string is 128 bytes long. We keep it twice:
		/// in linked list + dictionary. It is approximately 128 + 128 + 32 (additional expenses) ~ 288 bytes per hash.
		/// 50 * 288 = 14K seems reasonable amount of memory to allocate for 50 hashes.
		/// </summary>
		private const int TriggerCountStaleHashCheck = 50;

		private static readonly TimeSpan StaleHashPeriod = TimeSpan.FromMinutes(5.0);

		/// <summary>
		/// A wait handle that flags the sender when to start sending again. The type is protected for unit test.
		/// </summary>
		protected readonly AutoResetEvent DelayHandler;

		/// <summary>
		/// When storage is empty it will be queried again after this interval.
		/// </summary>
		private readonly TimeSpan sendingIntervalOnNoData = TimeSpan.FromSeconds(10.0);

		/// <summary>
		/// Holds the maximum time for the exponential back-off algorithm. The sending interval will grow on every HTTP Exception until this max value.
		/// </summary>
		private readonly TimeSpan maxIntervalBetweenRetries = TimeSpan.FromHours(1.0);

		/// <summary>
		/// A wait handle that is being set when Sender is no longer sending.
		/// </summary>
		private readonly AutoResetEvent stoppedHandler;

		/// <summary>
		/// Keep list of transmission hashes in order of most recent at the end. This allows us faster find
		/// all stale hashes.
		/// </summary>
		private readonly LinkedList<Tuple<DateTime, string>> listOfTransmissionHash = new LinkedList<Tuple<DateTime, string>>();

		/// <summary>
		/// Keeps list of transmission hashes in set, to be able to fast O(1) check whether hash in the set.
		/// </summary>
		private readonly Dictionary<string, LinkedListNode<Tuple<DateTime, string>>> setOfTransmissionHash = new Dictionary<string, LinkedListNode<Tuple<DateTime, string>>>();

		/// <summary>
		/// All operations on list and set of the transmission hash should be guarded by this lock,
		/// because we allow to work with Sender on a multiple threads.
		/// </summary>
		private readonly object hashLock = new object();

		/// <summary>
		/// Have this counter to understand whether start cleanup procedure.
		/// </summary>
		private int checkStaleHashCount;

		/// <summary>
		/// A boolean value that indicates if the sender should be stopped. The sender's while loop is checking this boolean value.
		/// </summary>
		private bool stopped;

		/// <summary>
		/// The amount of time to wait, in the stop method, until the last transmission is sent.
		/// If time expires, the stop method will return even if the transmission hasn't been sent.
		/// </summary>
		private TimeSpan drainingTimeout;

		/// <summary>
		/// The transmissions storage.
		/// </summary>
		private StorageBase storage;

		/// <summary>
		/// The number of times this object was disposed.
		/// </summary>
		private int disposeCount;

		/// <summary>
		/// Holds the transmitter.
		/// </summary>
		private PersistenceTransmitter transmitter;

		/// <summary>
		/// Gets the interval between each successful sending.
		/// </summary>
		private TimeSpan SendingInterval => transmitter.SendingInterval;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.Sender" /> class.
		/// </summary>
		/// <param name="storage">The storage that holds the transmissions to send.</param>
		/// <param name="transmitter">
		/// The persistence transmitter that manages this Sender.
		/// The transmitter will be used as a configuration class, it exposes properties like SendingInterval that will be read by Sender.
		/// </param>
		/// <param name="startSending">A boolean value that determines if Sender should start sending immediately. This is only used for unit tests.</param>
		internal Sender(StorageBase storage, PersistenceTransmitter transmitter, bool startSending = true)
		{
			stopped = false;
			DelayHandler = new AutoResetEvent(false);
			stoppedHandler = new AutoResetEvent(false);
			drainingTimeout = TimeSpan.FromSeconds(100.0);
			this.transmitter = transmitter;
			this.storage = storage;
			if (startSending)
			{
				Task.Factory.StartNew(SendLoop, TaskCreationOptions.LongRunning).ContinueWith(delegate(Task t)
				{
					CoreEventSource.Log.LogVerbose("Sender: Failure in SendLoop: Exception: " + t.Exception.ToString());
					LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", "Sender: Failure in SendLoop: Exception: " + t.Exception.Message);
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
		}

		/// <summary>
		/// Disposes the managed objects.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.Increment(ref disposeCount) == 1)
			{
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "Sender.Dispose() start disposing sender");
				StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				DelayHandler.Dispose();
				stoppedHandler.Dispose();
			}
		}

		/// <summary>
		/// Stops the sender.
		/// </summary>
		/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous operation.</returns>
		internal Task StopAsync()
		{
			stopped = true;
			DelayHandler.Set();
			return Task.Factory.StartNew(delegate
			{
				try
				{
					stoppedHandler.WaitOne(drainingTimeout);
				}
				catch (ObjectDisposedException)
				{
				}
			});
		}

		/// <summary>
		/// Try to upload all pending telemetry we have in the temporary folder.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		internal async Task FlushAll(CancellationToken token)
		{
			List<Task> list = new List<Task>();
			token.ThrowIfCancellationRequested();
			LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "Sender.FlushAll() start flushing all");
			foreach (StorageTransmission transmission in storage.PeekAll(token))
			{
				token.ThrowIfCancellationRequested();
				list.Add(Task.Run(async delegate
				{
					token.ThrowIfCancellationRequested();
					if (!(await SendAsync(transmission, token, default(TimeSpan)).ConfigureAwait(false)).Item1)
					{
						storage.Delete(transmission);
					}
					transmission.Dispose();
				}));
			}
			await Task.WhenAll(list).ConfigureAwait(false);
		}

		/// <summary>
		/// Send transmissions in a loop.
		/// </summary>
		protected void SendLoop()
		{
			TimeSpan prevSendInterval = TimeSpan.Zero;
			TimeSpan nextSendInterval = sendingIntervalOnNoData;
			try
			{
				for (; !stopped; LogInterval(prevSendInterval, nextSendInterval), DelayHandler.WaitOne(nextSendInterval), prevSendInterval = nextSendInterval)
				{
					using (StorageTransmission storageTransmission = storage.Peek())
					{
						if (!stopped)
						{
							if (storageTransmission != null)
							{
								if (LocalFileLoggerService.Default.Enabled)
								{
									LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceTransmitter.SendLoop about to send", new object[1]
									{
										storageTransmission
									}));
								}
								bool flag = Send(storageTransmission, ref nextSendInterval);
								if (LocalFileLoggerService.Default.Enabled)
								{
									LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceTransmitter.SendLoop shouldRetry == {1}", new object[2]
									{
										storageTransmission,
										flag
									}));
								}
								if (!flag)
								{
									storage.Delete(storageTransmission);
								}
							}
							else
							{
								nextSendInterval = sendingIntervalOnNoData;
							}
							continue;
						}
					}
					break;
				}
				stoppedHandler.Set();
			}
			catch (ObjectDisposedException)
			{
			}
		}

		/// <summary>
		/// Sends a transmission and handle errors.
		/// </summary>
		/// <param name="transmission">The transmission to send.</param>
		/// <param name="nextSendInterval">When this value returns it will hold a recommendation for when to start the next sending iteration.</param>
		/// <returns>A boolean value that indicates if there was a retriable error.</returns>
		protected virtual bool Send(StorageTransmission transmission, ref TimeSpan nextSendInterval)
		{
			Tuple<bool, TimeSpan> result = SendAsync(transmission, default(CancellationToken), nextSendInterval).ConfigureAwait(false).GetAwaiter().GetResult();
			nextSendInterval = result.Item2;
			return result.Item1;
		}

		/// <summary>
		/// Sends a transmission asynchronously and handle errors.
		/// </summary>
		/// <param name="transmission">The transmission to send.</param>
		/// <param name="token">Cancellation token</param>
		/// <param name="sendInterval">Previous send interval duration</param>
		/// <returns></returns>
		private async Task<Tuple<bool, TimeSpan>> SendAsync(StorageTransmission transmission, CancellationToken token, TimeSpan sendInterval)
		{
			bool isRetryable = false;
			try
			{
				if (transmission != null)
				{
					bool flag = true;
					lock (hashLock)
					{
						flag = !setOfTransmissionHash.ContainsKey(transmission.ContentHash);
						if (flag)
						{
							setOfTransmissionHash[transmission.ContentHash] = listOfTransmissionHash.AddLast(Tuple.Create(DateTime.UtcNow, transmission.ContentHash));
							checkStaleHashCount++;
							if (checkStaleHashCount >= 50)
							{
								CleanupStaleTransmissionHash();
								checkStaleHashCount = 0;
							}
						}
					}
					if (flag)
					{
						await transmission.SendAsync(token).ConfigureAwait(false);
					}
					sendInterval = SendingInterval;
				}
			}
			catch (WebException ex)
			{
				int? statusCode = GetStatusCode(ex);
				sendInterval = CalculateNextInterval(statusCode, sendInterval, maxIntervalBetweenRetries);
				isRetryable = IsRetryable(statusCode, ex.Status);
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceTransmitter.SendAsync WebException ({1}), isRetryable == {2}", new object[3]
				{
					transmission,
					ex.Message,
					isRetryable
				}));
			}
			catch (Exception ex2)
			{
				sendInterval = CalculateNextInterval(null, sendInterval, maxIntervalBetweenRetries);
				string message = string.Format(CultureInfo.InvariantCulture, "Unknown exception during sending: {0}", new object[1]
				{
					ex2
				});
				CoreEventSource.Log.LogVerbose(message);
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceTransmitter.SendAsync Exception ({1})", new object[2]
				{
					transmission,
					ex2.Message
				}));
				if (ex2 is OperationCanceledException)
				{
					throw;
				}
			}
			if (isRetryable)
			{
				lock (hashLock)
				{
					LinkedListNode<Tuple<DateTime, string>> value = null;
					if (setOfTransmissionHash.TryGetValue(transmission.ContentHash, out value) && value != null)
					{
						listOfTransmissionHash.Remove(value);
						setOfTransmissionHash.Remove(transmission.ContentHash);
					}
				}
			}
			return Tuple.Create(isRetryable, sendInterval);
		}

		/// <summary>
		/// Cleanup stale transmission hash based on the data hash is added.
		/// We need it to keep transmission hash set size not grow indefinitely to not waste memory.
		/// </summary>
		private void CleanupStaleTransmissionHash()
		{
			while (listOfTransmissionHash.Count > 0 && DateTime.UtcNow - listOfTransmissionHash.First.Value.Item1 > StaleHashPeriod)
			{
				setOfTransmissionHash.Remove(listOfTransmissionHash.First.Value.Item2);
				listOfTransmissionHash.RemoveFirst();
			}
		}

		/// <summary>
		/// Log next interval. Only log the interval when it changes by more then a minute. So if interval grow by 1 minute or decreased by 1 minute it will be logged.
		/// Logging every interval will just make the log noisy.
		/// </summary>
		private static void LogInterval(TimeSpan prevSendInterval, TimeSpan nextSendInterval)
		{
			if (Math.Abs(nextSendInterval.TotalSeconds - prevSendInterval.TotalSeconds) > 60.0)
			{
				CoreEventSource.Log.LogVerbose("next sending interval: " + nextSendInterval);
			}
		}

		/// <summary>
		/// Return the status code from the web exception or null if no such code exists.
		/// </summary>
		/// <returns></returns>
		private static int? GetStatusCode(WebException e)
		{
			return (int?)(e.Response as HttpWebResponse)?.StatusCode;
		}

		/// <summary>
		/// Returns true if <paramref name="httpStatusCode" /> or <paramref name="webExceptionStatus" /> are retriable.
		/// </summary>
		/// <returns></returns>
		private static bool IsRetryable(int? httpStatusCode, WebExceptionStatus webExceptionStatus)
		{
			if ((uint)(webExceptionStatus - 1) <= 1u || (uint)(webExceptionStatus - 14) <= 1u)
			{
				return true;
			}
			if (!httpStatusCode.HasValue)
			{
				return false;
			}
			switch (httpStatusCode.Value)
			{
			case 408:
			case 500:
			case 502:
			case 503:
			case 511:
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Calculates the next interval using exponential back-off algorithm (with the exceptions of few error codes that reset the interval to <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.Sender.SendingInterval" />.
		/// </summary>
		/// <returns></returns>
		private TimeSpan CalculateNextInterval(int? httpStatusCode, TimeSpan currentSendInterval, TimeSpan maxInterval)
		{
			if (httpStatusCode.HasValue && httpStatusCode.Value == 400)
			{
				return SendingInterval;
			}
			if (currentSendInterval.TotalSeconds == 0.0)
			{
				return TimeSpan.FromSeconds(1.0);
			}
			return TimeSpan.FromSeconds(Math.Min(currentSendInterval.TotalSeconds * 2.0, maxInterval.TotalSeconds));
		}
	}
}
