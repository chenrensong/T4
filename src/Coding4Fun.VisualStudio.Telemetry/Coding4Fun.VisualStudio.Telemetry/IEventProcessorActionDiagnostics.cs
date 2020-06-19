namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IEventProcessorActionDiagnostics
	{
		/// <summary>
		/// Allows the action to post diagnostic information when the manifest version
		/// changes or action is being disposed.
		/// </summary>
		/// <param name="mainSession"></param>
		/// <param name="newManifest"></param>
		void PostDiagnosticInformation(TelemetrySession mainSession, TelemetryManifest newManifest);
	}
}
