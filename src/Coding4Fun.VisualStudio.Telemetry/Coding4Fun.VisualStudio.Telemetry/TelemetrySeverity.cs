namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// An enum to define the severity of the telemetry event.
	/// It is used for any data consumer who wants to categorize data based on severity.
	/// </summary>
	public enum TelemetrySeverity
	{
		/// <summary>
		/// indicates telemetry event with high value or require attention (e.g., fault).
		/// </summary>
		High = 10,
		/// <summary>
		/// indicates a regular telemetry event.
		/// </summary>
		Normal = 0,
		/// <summary>
		/// indicates telemetry event with verbose information.
		/// </summary>
		Low = -10
	}
}
