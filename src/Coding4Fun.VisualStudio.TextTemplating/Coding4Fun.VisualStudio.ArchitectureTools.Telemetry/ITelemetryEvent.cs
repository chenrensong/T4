using Coding4Fun.VisualStudio.Telemetry;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public interface ITelemetryEvent
	{
		IEnumerable<DataPoint> SharedProperties
		{
			get;
		}

		double? Duration
		{
			get;
		}

		/// <summary>
		/// Writes a property on the wrapped activity.
		/// </summary>
		/// <param name="propertyIdentifier">Name of property to write.</param>
		/// <param name="propertyValue">Value of property to write.</param>
		/// <param name="shared">True if should share property with children</param>
		/// <param name="pii">True if property have personally identifiable information</param>
		void SetProperty(TelemetryIdentifier propertyIdentifier, object propertyValue, bool shared = false, bool pii = false);

		/// <summary>
		/// Writes properties on the wrapped activity.
		/// </summary>
		/// <param name="properties">DataPoints to write.</param>
		/// <param name="shared">True if should share properties with children</param>
		void SetProperties(IEnumerable<DataPoint> properties, bool shared = false);

		/// <summary>
		/// Correlate this event with other events via Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation.
		/// </summary>
		/// <param name="correlations"></param>
		void Correlate(params TelemetryEventCorrelation[] correlations);
	}
}
