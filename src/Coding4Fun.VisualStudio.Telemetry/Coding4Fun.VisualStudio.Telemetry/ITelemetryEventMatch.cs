namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Represents a telemetry event filter.
	/// </summary>
	public interface ITelemetryEventMatch
	{
		/// <summary>
		/// Indicates whether the specified <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent" /> satisfies this filter.
		/// </summary>
		/// <param name="telemetryEvent">The <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent" /> to check against this filter.</param>
		/// <returns>true if this filter is satisfied; otherwise, false.</returns>
		bool IsEventMatch(TelemetryEvent telemetryEvent);
	}
}
