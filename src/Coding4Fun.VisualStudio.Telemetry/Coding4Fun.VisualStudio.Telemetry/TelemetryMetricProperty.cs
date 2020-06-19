namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class represents a data model metric property.
	/// The property name will be updated with a suffix ".DataModelMetric" when the event is posted.
	/// A metric is a value or aggregated count collected as a measurement of a particular characteristic of the system.
	/// E.g., usage metrics like file size, project count, upload size; performance metric like duration.
	/// </summary>
	public class TelemetryMetricProperty : TelemetryDataModelProperty
	{
		/// <summary>
		/// Creates the Metric Property Object.
		/// </summary>
		/// <param name="val"></param>
		public TelemetryMetricProperty(double val)
			: base(val)
		{
		}
	}
}
