namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class MetricAction : DataModelPropertyAction<TelemetryMetricProperty>
	{
		public MetricAction()
			: base(50, ".DataModelMetric", "HasMetrics", "MetricProperties")
		{
		}
	}
}
