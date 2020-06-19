using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Telemetry for the experimentation service.
	/// </summary>
	public interface IExperimentationTelemetry
	{
		/// <summary>
		/// Set shared property for all events.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void SetSharedProperty(string name, string value);

		/// <summary>
		/// Opst one event with specified namd and property bag.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="properties"></param>
		void PostEvent(string name, IDictionary<string, string> properties);
	}
}
