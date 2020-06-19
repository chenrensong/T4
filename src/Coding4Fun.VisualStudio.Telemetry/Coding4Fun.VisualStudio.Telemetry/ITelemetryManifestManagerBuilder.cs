namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Builds a TelemetryManifestManager from the TelemetrySession.
	/// </summary>
	internal interface ITelemetryManifestManagerBuilder
	{
		ITelemetryManifestManager Build(TelemetrySession telemetrySession);
	}
}
