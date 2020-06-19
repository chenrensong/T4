using System;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Invalid match item is necessary for specify invalid (unrecognized) match item on JSON deserialization stage.
	/// During rule validation stage this item throws an exception and whole rule will be removed from the rule list.
	/// It allows us to partitially parse manifest files with new matching format.
	/// </summary>
	internal class TelemetryManifestInvalidMatchItem : ITelemetryManifestMatch, ITelemetryEventMatch
	{
		public IEnumerable<ITelemetryManifestMatch> GetChildren()
		{
			return Enumerable.Empty<ITelemetryManifestMatch>();
		}

		/// <summary>
		/// Check, whether does event match conditions
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <returns></returns>
		public bool IsEventMatch(TelemetryEvent telemetryEvent)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validates only 'this' class but not children. Explicity specify interface so that this method
		/// can only be called an instance typed to the interface and not an implementation type.
		/// </summary>
		void ITelemetryManifestMatch.ValidateItself()
		{
			throw new TelemetryManifestValidationException("invalid matching item");
		}
	}
}
