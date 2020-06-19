namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public class T4TelemetryProperty : TelemetryIdentifier
	{
		private const string PropertyPrefix = "tt.transform.";

		internal const string LanguageCsharp = "csharp";

		internal const string LanguageVB = "vb";

		internal static readonly T4TelemetryProperty CustomDirectivesProperty = new T4TelemetryProperty("tt.transform.", "customdirectives");

		internal static readonly T4TelemetryProperty LanguageProperty = new T4TelemetryProperty("tt.transform.", "language");

		public static readonly T4TelemetryProperty TransformAllProperty = new T4TelemetryProperty("tt.transform.", "transformall");

		private T4TelemetryProperty(string prefix, string value)
			: base(prefix, value)
		{
		}
	}
}
