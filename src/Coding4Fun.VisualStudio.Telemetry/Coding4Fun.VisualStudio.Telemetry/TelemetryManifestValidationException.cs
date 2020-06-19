using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Validation exception class, using in the validation of the manifest after deserializaing
	/// to check its correctness.
	/// </summary>
	internal class TelemetryManifestValidationException : Exception
	{
		public TelemetryManifestValidationException(string description)
			: base(description)
		{
		}
	}
}
