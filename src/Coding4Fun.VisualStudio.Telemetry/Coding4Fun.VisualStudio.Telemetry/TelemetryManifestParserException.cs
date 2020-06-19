using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Parser exception class, using in the manifest parsers
	/// </summary>
	internal class TelemetryManifestParserException : Exception
	{
		public TelemetryManifestParserException(string description, Exception innerException)
			: base(description, innerException)
		{
		}
	}
}
