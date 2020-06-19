using Coding4Fun.VisualStudio.ApplicationInsights.Channel;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// Represents an object that initializes <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> objects.
	/// </summary>
	/// <remarks>
	/// The <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" /> instances use <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.ITelemetryInitializer" /> objects to
	/// automatically initialize properties of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> objects.
	/// </remarks>
	public interface ITelemetryInitializer
	{
		/// <summary>
		/// Initializes properties of the specified <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> object.
		/// </summary>
		void Initialize(ITelemetry telemetry);
	}
}
