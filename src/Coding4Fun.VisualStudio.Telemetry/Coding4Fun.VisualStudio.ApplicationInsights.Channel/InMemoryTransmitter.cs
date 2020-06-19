using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// A transmitter that will immediately send telemetry over HTTP.
	/// Telemetry items are being sent when Flush is called, or when the buffer is full (An OnFull "event" is raised) or every 30 seconds.
	/// </summary>
	internal class InMemoryTransmitter : IDisposable
	{
		private readonly TelemetryBuffer buffer;

		/// <summary>
		/// A lock object to serialize the sending calls from Flush, OnFull event and the Runner.
		/// </summary>
		private object sendingLockObj = new object();

		private AutoResetEvent startRunnerEvent;

		private bool enabled = true;

		/// <summary>
		/// The number of times this object was disposed.
		/// </summary>
		private int disposeCount;

		private TimeSpan sendingInterval = TimeSpan.FromSeconds(30.0);

		private Uri endpointAddress = new Uri("https://dc.services.visualstudio.com/v2/track");

		internal Uri EndpointAddress
		{
			get
			{
				return endpointAddress;
			}
			set
			{
				Property.Set(endpointAddress, value);
			}
		}

		internal TimeSpan SendingInterval
		{
			get
			{
				return sendingInterval;
			}
			set
			{
				sendingInterval = value;
			}
		}

		internal InMemoryTransmitter(TelemetryBuffer buffer)
		{
			this.buffer = buffer;
			this.buffer.OnFull = OnBufferFull;
			Task.Factory.StartNew(Runner, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).ContinueWith(delegate(Task task)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "InMemoryTransmitter: Unhandled exception in Runner: {0}", new object[1]
				{
					task.Exception
				});
				CoreEventSource.Log.LogVerbose(message);
			}, TaskContinuationOptions.OnlyOnFaulted);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Flushes the in-memory buffer and sends it.
		/// </summary>
		internal void Flush()
		{
			DequeueAndSend();
		}

		/// <summary>
		/// Flushes the in-memory buffer and sends the telemetry items in <see cref="F:Coding4Fun.VisualStudio.ApplicationInsights.Channel.InMemoryTransmitter.sendingInterval" /> intervals or when
		/// <see cref="F:Coding4Fun.VisualStudio.ApplicationInsights.Channel.InMemoryTransmitter.startRunnerEvent" /> is set.
		/// </summary>
		private void Runner()
		{
			using (startRunnerEvent = new AutoResetEvent(false))
			{
				while (enabled)
				{
					DequeueAndSend();
					startRunnerEvent.WaitOne(sendingInterval);
				}
			}
		}

		/// <summary>
		/// Happens when the in-memory buffer is full. Flushes the in-memory buffer and sends the telemetry items.
		/// </summary>
		private void OnBufferFull()
		{
			startRunnerEvent.Set();
		}

		/// <summary>
		/// Flushes the in-memory buffer and send it.
		/// </summary>
		private void DequeueAndSend()
		{
			lock (sendingLockObj)
			{
				IEnumerable<ITelemetry> telemetryItems = buffer.Dequeue();
				try
				{
					Send(telemetryItems).ConfigureAwait(false).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					CoreEventSource.Log.LogVerbose("DequeueAndSend: Failed Sending: Exception: " + ex.ToString());
				}
			}
		}

		/// <summary>
		/// Serializes a list of telemetry items and sends them.
		/// </summary>
		/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous operation.</returns>
		private async Task Send(IEnumerable<ITelemetry> telemetryItems)
		{
			if (telemetryItems == null || !telemetryItems.Any())
			{
				CoreEventSource.Log.LogVerbose("No Telemetry Items passed to Enqueue");
				return;
			}
			byte[] content = JsonSerializer.Serialize(telemetryItems);
			await new Transmission(endpointAddress, content, "application/x-json-stream", JsonSerializer.CompressionType).SendAsync().ConfigureAwait(false);
		}

		private void Dispose(bool disposing)
		{
			if (Interlocked.Increment(ref disposeCount) == 1)
			{
				enabled = false;
				if (startRunnerEvent != null)
				{
					startRunnerEvent.Set();
				}
			}
		}
	}
}
