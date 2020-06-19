namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// Represents an object that supports initialization from <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration" />.
	/// </summary>
	public interface ISupportConfiguration
	{
		/// <summary>
		/// Initialize method is called after all configuration properties have been loaded from the configuration.
		/// </summary>
		void Initialize(TelemetryConfiguration configuration);
	}
}
