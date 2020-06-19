using Coding4Fun.VisualStudio.ApplicationInsights.Channel;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Default AppInsights channel. Post data to the AI backend directly.
	/// </summary>
	internal sealed class DefaultAppInsightsSessionChannel : BaseAppInsightsSessionChannel
	{
		private readonly IStorageBuilder storageBuilder;

		private readonly IProcessLockFactory processLockFactory;

		public override string ChannelId => "ai";

		public DefaultAppInsightsSessionChannel(string instrumentationKey, string userId)
			: this(instrumentationKey, userId, new WindowsStorageBuilder(), new WindowsProcessLockFactory())
		{
		}

		public DefaultAppInsightsSessionChannel(string instrumentationKey, string userId, IStorageBuilder storageBuilder, IProcessLockFactory processLockFactory)
			: base(instrumentationKey, userId)
		{
			this.storageBuilder = storageBuilder;
			this.processLockFactory = processLockFactory;
		}

		/// <summary>
		/// Obtain AppInsights client wrapper
		/// </summary>
		/// <returns></returns>
		protected override IAppInsightsClientWrapper CreateAppInsightsClientWrapper()
		{
			return new DefaultAppInsightsClientWrapper(InstrumentationKey, storageBuilder.Create(base.PersistenceFolderName), processLockFactory);
		}
	}
}
