using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// WatsonSessionChannelBuilder can build a WatsonSessionChannel.
	/// There is 2 stages of the creation. 1 stage is to create Builder itself with all necessary
	/// parameters to build a WatsonSessionChannel. Second stage is to Build a WatsonSessionChannel when TelemetrySession
	/// object is known.
	/// </summary>
	internal sealed class WatsonSessionChannelBuilder
	{
		private readonly int faultEventSamplePercent;

		private readonly int faultEventMaximumWatsonReportsPerSession;

		private readonly int faultEventMinimumSecondsBetweenWatsonReports;

		private readonly ChannelProperties properties;

		public WatsonSessionChannel WatsonSessionChannel
		{
			get;
			private set;
		}

		public WatsonSessionChannelBuilder(int faultEventSamplePercent, int faultEventMaximumWatsonReportsPerSession, int faultEventMinimumSecondsBetweenWatsonReports, ChannelProperties properties)
		{
			this.faultEventSamplePercent = faultEventSamplePercent;
			this.faultEventMaximumWatsonReportsPerSession = faultEventMaximumWatsonReportsPerSession;
			this.faultEventMinimumSecondsBetweenWatsonReports = faultEventMinimumSecondsBetweenWatsonReports;
			this.properties = properties;
		}

		/// <summary>
		/// Build WatsonChannelBuilder and all its dependencies
		/// </summary>
		/// <param name="hostSession"></param>
		public void Build(TelemetrySession hostSession)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(hostSession, "hostSession");
			WatsonSessionChannel = new WatsonSessionChannel(hostSession, faultEventSamplePercent, faultEventMaximumWatsonReportsPerSession, faultEventMinimumSecondsBetweenWatsonReports)
			{
				Properties = properties
			};
		}
	}
}
