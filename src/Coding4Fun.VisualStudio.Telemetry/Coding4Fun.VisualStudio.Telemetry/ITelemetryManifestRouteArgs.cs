namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// ITelemetryManifestRouteArgs interface for the providing arguments to the router
	/// </summary>
	public interface ITelemetryManifestRouteArgs
	{
		/// <summary>
		/// Validate arguments on post-parsing stage to avoid validate
		/// each time during posting events.
		/// </summary>
		void Validate();
	}
}
