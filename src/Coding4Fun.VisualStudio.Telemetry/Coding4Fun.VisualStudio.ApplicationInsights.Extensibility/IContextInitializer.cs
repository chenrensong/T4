using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// Represents an object that implements supporting logic for <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
	/// </summary>
	/// <remarks>
	/// One type of objects that support <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" /> is a telemetry source.
	/// A telemetry source can supply initial property values for a <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" /> object
	/// during its construction or generate <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> objects during its lifetime.
	/// </remarks>
	public interface IContextInitializer
	{
		/// <summary>
		/// Initializes the given <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		void Initialize(TelemetryContext context);
	}
}
