using System.Diagnostics.Tracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Represents a communication channel for sending telemetry to Application Insights via UTC (Windows Universal Telemetry Client).
	/// </summary>
	public sealed class UniversalTelemetryChannel : ITelemetryChannel, IDisposable
	{
		private readonly ConcurrentDictionary<string, EventSourceWriter> eventSourceWriters;

		private bool disposed;

		/// <summary>
		/// Gets or sets a value indicating whether developer mode of telemetry transmission is enabled.
		/// When developer mode is True, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.UniversalTelemetryChannel" /> sends telemetry to Application Insights immediately
		/// during the entire lifetime of the application. When developer mode is False, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.UniversalTelemetryChannel" />
		/// respects production sending policies defined by other properties.
		/// </summary>
		public bool DeveloperMode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating the endpoint address. This property is ignored.
		/// </summary>
		public string EndpointAddress
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.UniversalTelemetryChannel" /> class.
		/// </summary>
		public UniversalTelemetryChannel()
		{
			eventSourceWriters = new ConcurrentDictionary<string, EventSourceWriter>();
		}

		/// <summary>
		/// Returns true if the channel is available to use.
		/// </summary>
		/// <returns></returns>
		public static bool IsAvailable()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			EventSource val = (EventSource)(object)new EventSource("Coding4Fun-Windows-UTC-Presence");
			try
			{
				return val.IsEnabled();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
			GetEventSourceWriter(item.Context.InstrumentationKey).WriteTelemetry(item);
		}

		/// <summary>
		/// No-op because every <see cref="M:Coding4Fun.VisualStudio.ApplicationInsights.Channel.UniversalTelemetryChannel.Send(Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry)" /> method is immediately calling UTC. So every call is immediately "flushed" to the UTC agent.
		/// </summary>
		public void Flush()
		{
		}

		/// <summary>
		/// Flushes the in-memory buffer and transmit data asynchronously.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public async Task FlushAndTransmitAsync(CancellationToken token)
		{
			await Task.FromResult(true);
		}

		internal EventSourceWriter GetEventSourceWriter(string instrumentationKey)
		{
			return eventSourceWriters.GetOrAdd(instrumentationKey, (string key) => new EventSourceWriter(key, DeveloperMode));
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					foreach (KeyValuePair<string, EventSourceWriter> eventSourceWriter in eventSourceWriters)
					{
						eventSourceWriter.Value.Dispose();
					}
				}
				disposed = true;
			}
		}
	}
}
