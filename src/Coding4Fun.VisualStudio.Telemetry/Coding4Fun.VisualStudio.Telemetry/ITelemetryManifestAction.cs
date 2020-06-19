namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryManifestAction : IEventProcessorAction
	{
		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		void Validate();
	}
}
