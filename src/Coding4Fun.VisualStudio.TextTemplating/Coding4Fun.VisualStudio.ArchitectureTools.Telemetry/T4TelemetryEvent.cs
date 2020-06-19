namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public class T4TelemetryEvent : TelemetryIdentifier
	{
		private const string EventNamePrefix = "vs/architecturetools/tt/";

		internal static readonly T4TelemetryEvent ProcessTemplate = new T4TelemetryEvent("vs/architecturetools/tt/", "process-template");

		internal static readonly T4TelemetryEvent PreprocessTemplate = new T4TelemetryEvent("vs/architecturetools/tt/", "preprocess-template");

		internal static readonly T4TelemetryEvent DebugTemplate = new T4TelemetryEvent("vs/architecturetools/tt/", "debug-template");

		internal static readonly T4TelemetryEvent TransformationFault = new T4TelemetryEvent("vs/architecturetools/tt/", "transformation-fault");

		internal static readonly T4TelemetryEvent ParseOperation = new T4TelemetryEvent("vs/architecturetools/tt/", "parse-template");

		internal static readonly T4TelemetryEvent CompilationOperation = new T4TelemetryEvent("vs/architecturetools/tt/", "compilation-template");

		internal static readonly T4TelemetryEvent TransformationOperation = new T4TelemetryEvent("vs/architecturetools/tt/", "transformation-template");

		public static readonly T4TelemetryEvent TransformAll = new T4TelemetryEvent("vs/architecturetools/tt/", "transform-all");

		private T4TelemetryEvent(string prefix, string value)
			: base(prefix, value)
		{
		}
	}
}
