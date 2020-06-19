namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Invalid route args item is necessary for specify invalid (unrecognized)
	/// route args on JSON deserialization stage.
	/// During rule validation stage this route action should be ignored and removed from the action list.
	/// It allows us to partitially parse manifest files with new actions.
	/// </summary>
	internal class TelemetryManifestInvalidRouteArgs : ITelemetryManifestRouteArgs
	{
		/// <summary>
		/// Validate arguments on post-parsing stage to avoid validate
		/// each time during posting events.
		/// </summary>
		public void Validate()
		{
			throw new TelemetryManifestValidationException("invalid route arguments");
		}
	}
}
