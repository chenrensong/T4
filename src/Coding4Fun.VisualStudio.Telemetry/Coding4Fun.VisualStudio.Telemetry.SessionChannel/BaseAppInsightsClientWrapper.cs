using Coding4Fun.VisualStudio.ApplicationInsights;
using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Base AppInsightsClientWrapper. Implement all necessary functionality for the work with AI channel.
	/// Derived by real channels: Default, Asimov
	/// </summary>
	internal abstract class BaseAppInsightsClientWrapper : TelemetryDisposableObject, IAppInsightsClientWrapper, IDisposable, IDisposeAndTransmit
	{
		private readonly string instrumentationKey;

		private TelemetryClient appInsightsClient;

		private ITelemetryChannel appInsightsChannel;

		public string InstrumentationKey => instrumentationKey;

		public abstract bool TryGetTransport(out string transportUsed);

		public BaseAppInsightsClientWrapper(string instrumentationKey)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(instrumentationKey, "instrumentationKey");
			this.instrumentationKey = instrumentationKey;
		}

		public void Initialize(string sessionId, string userId)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(sessionId, "sessionId");
			TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
			if (telemetryConfiguration.TelemetryChannel != null)
			{
				telemetryConfiguration.TelemetryChannel.Dispose();
				telemetryConfiguration.TelemetryChannel = null;
			}
			telemetryConfiguration.TelemetryInitializers.Remove(telemetryConfiguration.TelemetryInitializers.FirstOrDefault((ITelemetryInitializer o) => o is TimestampPropertyInitializer));
			appInsightsChannel = CreateAppInsightsChannel(telemetryConfiguration);
			TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);
			Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext context = telemetryClient.Context;
			context.InstrumentationKey = InstrumentationKey;
			context.Session.Id = sessionId;
			context.User.Id = userId;
			context.Device.Type = "0";
			context.Device.Id = "0";
			appInsightsClient = telemetryClient;
		}

		/// <summary>
		/// Send the specified telemetry event to Application Insights.
		/// </summary>
		/// <param name="ev">Event</param>
		public void TrackEvent(EventTelemetry ev)
		{
			if (appInsightsClient != null && !base.IsDisposed)
			{
				appInsightsClient.TrackEvent(ev);
			}
		}

		/// <summary>
		/// Transmit all internal buffers to the end-point and dispose channel.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns></returns>
		public async Task DisposeAndTransmitAsync(CancellationToken token)
		{
			base.DisposeManagedResources();
			if (appInsightsChannel != null)
			{
				try
				{
					await appInsightsClient.FlushAndTransmitAsync(token).ConfigureAwait(false);
				}
				catch (FileNotFoundException)
				{
				}
				DisposeChannel();
			}
		}

		/// <summary>
		/// Create real AppInsights transport channel
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		protected abstract ITelemetryChannel CreateAppInsightsChannel(TelemetryConfiguration config);

		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			if (appInsightsChannel != null)
			{
				try
				{
					appInsightsClient.Flush();
				}
				catch (FileNotFoundException)
				{
				}
				DisposeChannel();
			}
		}

		private void DisposeChannel()
		{
			try
			{
				appInsightsChannel.Dispose();
			}
			catch (InvalidOperationException)
			{
			}
			catch (FileNotFoundException)
			{
			}
		}
	}
}
