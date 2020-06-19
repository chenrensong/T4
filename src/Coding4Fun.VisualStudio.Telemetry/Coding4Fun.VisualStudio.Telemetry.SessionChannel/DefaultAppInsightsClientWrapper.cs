using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Default client wrapper - create direct AI channel
	/// </summary>
	internal sealed class DefaultAppInsightsClientWrapper : BaseAppInsightsClientWrapper
	{
		/// <summary>
		/// MaxTransmissionBufferCapacity for the internal transmission buffer of the AI
		/// Default value is 1M, we reduced it in order to reduce shutdown time,
		/// when AI writes unsent events to the disk.
		/// </summary>
		private const int MaxTransmissionBufferCapacity = 102400;

		private readonly StorageBase storage;

		private readonly IProcessLockFactory processLockFactory;

		public DefaultAppInsightsClientWrapper(string instrumentationKey, StorageBase storage, IProcessLockFactory processLockFactory)
			: base(instrumentationKey)
		{
			this.storage = storage;
			this.processLockFactory = processLockFactory;
		}

		/// <summary>
		/// For AI channel no specific transport is used, so we return false
		/// </summary>
		/// <param name="transportUsed"></param>
		/// <returns></returns>
		public override bool TryGetTransport(out string transportUsed)
		{
			transportUsed = null;
			return false;
		}

		/// <summary>
		/// Create default InProcess channel
		/// </summary>
		/// <param name="config"></param>
		/// <returns></returns>
		protected override ITelemetryChannel CreateAppInsightsChannel(TelemetryConfiguration config)
		{
			PersistenceChannel persistenceChannel = new PersistenceChannel(storage, processLockFactory);
			persistenceChannel.Initialize(config);
			config.TelemetryChannel = persistenceChannel;
			return config.TelemetryChannel;
		}
	}
}
