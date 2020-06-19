using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// This class handles all the logic for flushing the In Memory buffer to the persistent storage.
	/// </summary>
	internal class FlushManager : IDisposable
	{
		/// <summary>
		/// The memory buffer.
		/// </summary>
		private readonly TelemetryBuffer telemetryBuffer;

		/// <summary>
		/// A wait handle that signals when a flush is required.
		/// </summary>
		private AutoResetEvent flushWaitHandle;

		/// <summary>
		/// The storage that is used to persist all the transmissions.
		/// </summary>
		private StorageBase storage;

		/// <summary>
		/// The number of times this object was disposed.
		/// </summary>
		private int disposeCount;

		/// <summary>
		/// A boolean value that determines if the long running thread that runs flush in a loop will stay alive.
		/// </summary>
		private bool flushLoopEnabled = true;

		/// <summary>
		/// Gets or sets the maximum telemetry batching interval. Once the interval expires, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceTransmitter" />
		/// persists the accumulated telemetry items.
		/// </summary>
		internal TimeSpan FlushDelay
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the service endpoint.
		/// </summary>
		/// <remarks>
		/// Q: Why flushManager knows about the endpoint?
		/// A: Storage stores <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.Transmission" /> objects and Transmission objects contain the endpoint address.
		/// </remarks>
		internal Uri EndpointAddress
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.FlushManager" /> class.
		/// </summary>
		/// <param name="storage">The storage that persists the telemetries.</param>
		/// <param name="telemetryBuffer">In memory buffer that holds telemetries.</param>
		/// <param name="supportAutoFlush">A boolean value that determines if flush will happen automatically. Used by unit tests.</param>
		internal FlushManager(StorageBase storage, TelemetryBuffer telemetryBuffer, bool supportAutoFlush = true)
		{
			this.storage = storage;
			this.telemetryBuffer = telemetryBuffer;
			this.telemetryBuffer.OnFull = OnTelemetryBufferFull;
			FlushDelay = TimeSpan.FromSeconds(30.0);
			if (supportAutoFlush)
			{
				Task.Factory.StartNew(FlushLoop, TaskCreationOptions.LongRunning).ContinueWith(delegate(Task t)
				{
					CoreEventSource.Log.LogVerbose("FlushManager: Failure in FlushLoop: Exception: " + t.Exception.ToString());
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
		}

		/// <summary>
		/// Disposes the object.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.Increment(ref disposeCount) == 1 && flushWaitHandle != null)
			{
				flushLoopEnabled = false;
				flushWaitHandle.Set();
			}
		}

		/// <summary>
		/// Persist the in-memory telemetry items.
		/// </summary>
		internal void Flush()
		{
			IEnumerable<ITelemetry> enumerable = telemetryBuffer.Dequeue();
			if (enumerable != null && enumerable.Any())
			{
				byte[] content = JsonSerializer.Serialize(enumerable);
				Transmission transmission = new Transmission(EndpointAddress, content, "application/x-json-stream", JsonSerializer.CompressionType);
				storage.EnqueueAsync(transmission).ConfigureAwait(false).GetAwaiter()
					.GetResult();
			}
		}

		/// <summary>
		/// Flushes in intervals set by <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.FlushManager.FlushDelay" />.
		/// </summary>
		private void FlushLoop()
		{
			using (flushWaitHandle = new AutoResetEvent(false))
			{
				while (flushLoopEnabled)
				{
					Flush();
					flushWaitHandle.WaitOne(FlushDelay);
				}
			}
		}

		/// <summary>
		/// Handles the full event coming from the TelemetryBuffer.
		/// </summary>
		private void OnTelemetryBufferFull()
		{
			if (flushWaitHandle != null && flushLoopEnabled)
			{
				flushWaitHandle.Set();
			}
		}
	}
}
