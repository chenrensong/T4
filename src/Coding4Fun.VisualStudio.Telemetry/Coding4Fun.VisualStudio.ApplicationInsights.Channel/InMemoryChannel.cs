using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Represents a communication channel for sending telemetry to Application Insights via HTTPS. There will be a buffer that will not be persisted, to enforce the
	/// queued telemetry items to be sent, <see cref="M:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetryChannel.Flush" /> should be caller.
	/// </summary>
	public class InMemoryChannel : ITelemetryChannel, IDisposable
	{
		private readonly TelemetryBuffer buffer;

		private readonly InMemoryTransmitter transmitter;

		private bool developerMode;

		private int bufferSize;

		/// <summary>
		/// Gets or sets a value indicating whether developer mode of telemetry transmission is enabled.
		/// </summary>
		public bool DeveloperMode
		{
			get
			{
				return developerMode;
			}
			set
			{
				if (value != developerMode)
				{
					if (value)
					{
						bufferSize = buffer.Capacity;
						buffer.Capacity = 1;
					}
					else
					{
						buffer.Capacity = bufferSize;
					}
					developerMode = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the sending interval. Once the interval expires, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.InMemoryChannel" />
		/// serializes the accumulated telemetry items for transmission and sends it over the wire.
		/// </summary>
		public TimeSpan SendingInterval
		{
			get
			{
				return transmitter.SendingInterval;
			}
			set
			{
				transmitter.SendingInterval = value;
			}
		}

		/// <summary>
		/// Gets or sets the HTTP address where the telemetry is sent.
		/// </summary>
		public string EndpointAddress
		{
			get
			{
				return transmitter.EndpointAddress.ToString();
			}
			set
			{
				transmitter.EndpointAddress = new Uri(value);
			}
		}

		/// <summary>
		/// Gets or sets the maximum telemetry batching interval. Once the interval expires, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.InMemoryChannel" />
		/// serializes the accumulated telemetry items for transmission.
		/// </summary>
		[Obsolete("This value is now obsolete and will be removed in next release, use SendingInterval instead.")]
		public double DataUploadIntervalInSeconds
		{
			get
			{
				return transmitter.SendingInterval.TotalSeconds;
			}
			set
			{
				transmitter.SendingInterval = TimeSpan.FromSeconds(value);
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of telemetry items will accumulate in a memory before
		/// the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.InMemoryChannel" /> serializing them for transmission to Application Insights.
		/// </summary>
		public int MaxTelemetryBufferCapacity
		{
			get
			{
				return buffer.Capacity;
			}
			set
			{
				buffer.Capacity = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.InMemoryChannel" /> class.
		/// </summary>
		public InMemoryChannel()
		{
			buffer = new TelemetryBuffer();
			transmitter = new InMemoryTransmitter(buffer);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.InMemoryChannel" /> class. Used in unit tests for constructor injection.
		/// </summary>
		/// <param name="telemetryBuffer">The telemetry buffer that will be used to enqueue new events.</param>
		/// <param name="transmitter">The in memory transmitter that will send the events queued in the buffer.</param>
		internal InMemoryChannel(TelemetryBuffer telemetryBuffer, InMemoryTransmitter transmitter)
		{
			buffer = telemetryBuffer;
			this.transmitter = transmitter;
		}

		/// <summary>
		/// Sends an instance of ITelemetry through the channel.
		/// </summary>
		public void Send(ITelemetry item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("item");
			}
			try
			{
				buffer.Enqueue(item);
				CoreEventSource.Log.LogVerbose("TelemetryBuffer.Enqueue succeeded");
			}
			catch (Exception ex)
			{
				CoreEventSource.Log.LogVerbose("TelemetryBuffer.Enqueue failed: ", ex.ToString());
			}
		}

		/// <summary>
		/// Will send all the telemetry items stored in the memory.
		/// </summary>
		public void Flush()
		{
			transmitter.Flush();
		}

		/// <summary>
		/// Flushes the in-memory buffer and transmit data asynchronously.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task FlushAndTransmitAsync(CancellationToken token)
		{
			transmitter.Flush();
			await Task.FromResult(true);
		}

		/// <summary>
		/// Disposing the channel.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing && transmitter != null)
			{
				transmitter.Dispose();
			}
		}
	}
}
