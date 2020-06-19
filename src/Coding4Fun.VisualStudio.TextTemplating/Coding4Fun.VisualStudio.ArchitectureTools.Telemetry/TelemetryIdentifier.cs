namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public class TelemetryIdentifier
	{
		public string Value
		{
			get;
		}

		protected internal TelemetryIdentifier(string prefix, string name)
		{
			Value = prefix + name;
		}

		public override string ToString()
		{
			return Value;
		}
	}
}
