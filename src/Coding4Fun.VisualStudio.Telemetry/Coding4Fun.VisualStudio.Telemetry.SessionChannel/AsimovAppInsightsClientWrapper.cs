using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
    /// <summary>
    /// Default client wrapper - create direct AI channel
    /// </summary>
    internal sealed class AsimovAppInsightsClientWrapper : BaseAppInsightsClientWrapper
	{
		private enum Transport
		{
			Utc,
			Vortex
		}

		/// <summary>
		/// MaxTransmissionBufferCapacity for the internal transmission buffer of the AI
		/// Default value is 1M, we reduced it in order to reduce shutdown time,
		/// when AI writes unsent events to the disk.
		/// </summary>
		private const int MaxTransmissionBufferCapacity = 102400;

		private const string TransportUtc = "utc";

		private const string TransportVortex = "vortex";

		private const string UtcInstalledYes = "Yes";

		private const string UtcInstalledNo = "No";

		private const string UtcInstalledUnknown = "Unknown";

		private readonly bool isUtcEnabled;

		private readonly TelemetrySession hostTelemetrySession;

		private readonly StorageBase storage;

		private readonly IProcessLockFactory processLockFactory;

		private Transport usedTransport;

		public AsimovAppInsightsClientWrapper(bool isUtcEnabled, string instrumentationKey, TelemetrySession hostTelemetrySession, StorageBase storage, IProcessLockFactory processLockFactory)
			: base(instrumentationKey)
		{
			this.isUtcEnabled = isUtcEnabled;
			this.hostTelemetrySession = hostTelemetrySession;
			this.storage = storage;
			this.processLockFactory = processLockFactory;
		}

		/// <summary>
		/// Return current transport
		/// </summary>
		/// <param name="transportUsed"></param>
		/// <returns></returns>
		public override bool TryGetTransport(out string transportUsed)
		{
			if (isUtcEnabled)
			{
				transportUsed = ((usedTransport == Transport.Vortex) ? "vortex" : "utc");
				return true;
			}
			transportUsed = null;
			return false;
		}

		/// <summary>
		/// Create Asimov AI channel
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		protected override ITelemetryChannel CreateAppInsightsChannel(TelemetryConfiguration config)
		{
			config.TelemetryChannel = null;
			string text = "Unknown";
			text = "No";
			if (isUtcEnabled)
			{
				bool num = UniversalTelemetryChannel.IsAvailable();
				text = (num ? "Yes" : "No");
				if (num)
				{
					config.TelemetryChannel = new UniversalTelemetryChannel();
					usedTransport = Transport.Utc;
				}
			}
			if (config.TelemetryChannel == null)
			{
				PersistenceChannel persistenceChannel = new PersistenceChannel(storage, processLockFactory);
				persistenceChannel.Initialize(config);
				persistenceChannel.EndpointAddress = "https://vortex.data.microsoft.com/collect/v1";
				config.TelemetryChannel = persistenceChannel;
				config.TelemetryInitializers.Add(new SequencePropertyInitializer());
				usedTransport = Transport.Vortex;
			}
			if (!hostTelemetrySession.IsSessionCloned)
			{
				hostTelemetrySession.PostProperty("VS.TelemetryApi.IsUtcInstalled", text);
			}
			return config.TelemetryChannel;
		}
	}
}
