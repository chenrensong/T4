using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryManifestMatch : ITelemetryEventMatch
	{
		/// <summary>
		/// Validates only 'this' class but not children.
		/// </summary>
		void ValidateItself();

		/// <summary>
		/// Returns all children.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ITelemetryManifestMatch> GetChildren();
	}
}
