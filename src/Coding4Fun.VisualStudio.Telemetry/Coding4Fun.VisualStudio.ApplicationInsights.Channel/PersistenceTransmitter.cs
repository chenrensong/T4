using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using Coding4Fun.VisualStudio.LocalLogger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Implements throttled and persisted transmission of telemetry to Application Insights.
	/// </summary>
	internal class PersistenceTransmitter : IDisposable
	{
		/// <summary>
		/// A locker that will be used as a name mutex to synchronize transmitters from different channels and different processes.
		/// </summary>
		private readonly IProcessLock locker;

		/// <summary>
		/// A list of senders that sends transmissions.
		/// </summary>
		private ConcurrentBag<Sender> senders = new ConcurrentBag<Sender>();

		/// <summary>
		/// The storage that is used to persist all the transmissions.
		/// </summary>
		private StorageBase storage;

		/// <summary>
		/// Cancels the sending.
		/// </summary>
		private CancellationTokenSource sendingCancellationTokenSource;

		/// <summary>
		/// The number of times this object was disposed.
		/// </summary>
		private int disposeCount;

		/// <summary>
		/// Mutex is released once the thread that acquired it is ended. This event keeps the long running thread that acquire the mutex alive until dispose is called.
		/// </summary>
		private AutoResetEvent eventToKeepMutexThreadAlive;

		/// <summary>
		/// Gets a unique folder name. This folder will be used to store the transmission files.
		/// </summary>
		internal string StorageUniqueFolder => storage.FolderName;

		/// <summary>
		/// Gets or sets the interval between each successful sending.
		/// </summary>
		internal TimeSpan SendingInterval
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceTransmitter" /> class.
		/// </summary>
		/// <param name="storage">The transmissions storage.</param>
		/// <param name="sendersCount">The number of senders to create.</param>
		/// <param name="createSenders">A boolean value that indicates if this class should try and create senders. This is a workaround for unit tests purposes only.</param>
		internal PersistenceTransmitter(StorageBase storage, int sendersCount, bool createSenders = true)
			: this(storage, sendersCount, new WindowsProcessLockFactory(), createSenders)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceTransmitter" /> class.
		/// </summary>
		/// <param name="storage">The transmissions storage.</param>
		/// <param name="sendersCount">The number of senders to create.</param>
		/// <param name="processLockFactory">IProcessLockBuilder that will create an IProcessLock to sync transmission between processes</param>
		/// <param name="createSenders">A boolean value that indicates if this class should try and create senders. This is a workaround for unit tests purposes only.</param>
		internal PersistenceTransmitter(StorageBase storage, int sendersCount, IProcessLockFactory processLockFactory, bool createSenders = true)
		{
			this.storage = storage;
			sendingCancellationTokenSource = new CancellationTokenSource();
			eventToKeepMutexThreadAlive = new AutoResetEvent(false);
			try
			{
				string text = this.storage.StorageFolder?.FullName;
				if (text == null)
				{
					text = string.Empty;
				}
				locker = processLockFactory.CreateLocker(text, "_675531BB6E734D2F846AB8511A8963FD_");
			}
			catch (Exception ex)
			{
				locker = null;
				string message = string.Format(CultureInfo.InvariantCulture, "PersistenceTransmitter: Failed to construct the mutex: {0}", new object[1]
				{
					ex
				});
				CoreEventSource.Log.LogVerbose(message);
			}
			if (createSenders)
			{
				Task.Factory.StartNew(delegate
				{
					AcquireMutex(delegate
					{
						CreateSenders(sendersCount);
					});
				}, TaskCreationOptions.LongRunning).ContinueWith(delegate(Task task)
				{
					string text2 = string.Format(CultureInfo.InvariantCulture, "PersistenceTransmitter: Unhandled exception in CreateSenders: {0}", new object[1]
					{
						task.Exception
					});
					LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", text2);
					CoreEventSource.Log.LogVerbose(text2);
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
		}

		public async Task Flush(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			if (senders.TryPeek(out Sender result) && result != null)
			{
				await result.FlushAll(token).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Disposes the object.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.Increment(ref disposeCount) == 1)
			{
				sendingCancellationTokenSource.Cancel();
				sendingCancellationTokenSource.Dispose();
				locker?.Dispose();
				eventToKeepMutexThreadAlive.Dispose();
				StopSenders();
			}
		}

		/// <summary>
		/// Make sure that <paramref name="action" /> happens only once even if it is executed on different processes.
		/// On every given time only one channel will acquire the mutex, even if the channel is on a different process.
		/// This method is using a named mutex to achieve that. Once the mutex is acquired <paramref name="action" /> will be executed.
		/// </summary>
		/// <param name="action">The action to perform once the mutex is acquired.</param>
		private void AcquireMutex(Action action)
		{
			if (locker != null)
			{
				while (!sendingCancellationTokenSource.IsCancellationRequested)
				{
					try
					{
						LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "PersistenceTransmitter.AcquireMutex try to acquire mutex");
						locker.Acquire(action, sendingCancellationTokenSource.Token);
						if (!sendingCancellationTokenSource.IsCancellationRequested)
						{
							LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "PersistenceTransmitter.AcquireMutex mutex acquired successfully and action executed");
							eventToKeepMutexThreadAlive.WaitOne();
							LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "PersistenceTransmitter.AcquireMutex exit from the thread naturally");
						}
						return;
					}
					catch (ObjectDisposedException)
					{
						LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", "PersistenceTransmitter.AcquireMutex exit from the thread by object disposed exception");
						return;
					}
					catch (ProcessLockException ex2)
					{
						if (!ex2.IsRetryable)
						{
							LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", "PersistenceTransmitter.AcquireMutex exit from the thread by non-retriebale ProcessLockException");
							return;
						}
					}
				}
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "PersistenceTransmitter.AcquireMutex exit from the thread because of the cancellation token source");
			}
		}

		/// <summary>
		/// Create senders to send telemetries.
		/// </summary>
		private void CreateSenders(int sendersCount)
		{
			LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "PersistenceTransmitter.CreateSenders start creating senders");
			for (int i = 0; i < sendersCount; i++)
			{
				senders.Add(new Sender(storage, this));
			}
			LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", "PersistenceTransmitter.CreateSenders finished creating senders");
		}

		/// <summary>
		/// Stops the senders.
		/// </summary>
		/// <remarks>As long as there is no Start implementation, this method should only be called from Dispose.</remarks>
		private void StopSenders()
		{
			if (senders != null)
			{
				List<Task> list = new List<Task>();
				foreach (Sender sender in senders)
				{
					list.Add(sender.StopAsync());
				}
				Task.WaitAll(list.ToArray());
			}
		}
	}
}
