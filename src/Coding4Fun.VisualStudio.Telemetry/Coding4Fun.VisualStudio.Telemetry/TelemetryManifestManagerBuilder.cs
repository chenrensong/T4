using Coding4Fun.VisualStudio.RemoteControl;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Builds a TelemetryManifestManager from the telemetrySession
	/// </summary>
	internal sealed class TelemetryManifestManagerBuilder : ITelemetryManifestManagerBuilder
	{
		private readonly object remoteControlClient;

		private readonly ITelemetryManifestManagerSettings settings;

		private readonly ITelemetryManifestParser manifestParser;

		private readonly ITelemetryScheduler scheduler;

		/// <summary>
		/// Constructs a builder using defaults for some of the member variables. The null variables will be populated later.
		/// </summary>
		public TelemetryManifestManagerBuilder()
			: this(null, null, new JsonTelemetryManifestParser(), new TelemetryScheduler())
		{
		}

		/// <summary>
		/// Constructs a builder using explicitly supplied member variables.
		/// </summary>
		/// <param name="theRemoteControlClient"></param>
		/// <param name="theSettings"></param>
		/// <param name="theManifestParser"></param>
		/// <param name="theScheduler"></param>
		public TelemetryManifestManagerBuilder(object theRemoteControlClient, ITelemetryManifestManagerSettings theSettings, ITelemetryManifestParser theManifestParser, ITelemetryScheduler theScheduler)
		{
			remoteControlClient = theRemoteControlClient;
			settings = theSettings;
			manifestParser = theManifestParser;
			scheduler = theScheduler;
		}

		/// <summary>
		/// Builds the manifest manager using the telemetrySession.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <returns></returns>
		public ITelemetryManifestManager Build(TelemetrySession telemetrySession)
		{
			return new TelemetryManifestManager(remoteControlClient as IRemoteControlClient, settings, manifestParser, scheduler, telemetrySession);
		}
	}
}
