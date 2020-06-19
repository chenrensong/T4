namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public class DataPoint
	{
		public TelemetryIdentifier Identity
		{
			get;
			private set;
		}

		public object Value
		{
			get;
			set;
		}

		public DataPoint(TelemetryIdentifier name, object value)
		{
			Identity = name;
			Value = value;
		}
	}
}
