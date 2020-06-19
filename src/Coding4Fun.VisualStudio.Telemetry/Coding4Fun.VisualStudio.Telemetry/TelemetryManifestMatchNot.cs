using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchNot : ITelemetryManifestMatch, ITelemetryEventMatch
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public ITelemetryManifestMatch Not
		{
			get;
			set;
		}

		public IEnumerable<ITelemetryManifestMatch> GetChildren()
		{
			yield return Not;
		}

		/// <summary>
		/// Check, whether does event match conditions
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <returns></returns>
		public bool IsEventMatch(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			return !Not.IsEventMatch(telemetryEvent);
		}

		/// <summary>
		/// Validates only 'this' class but not children. Explicity specify interface so that this method
		/// can only be called an instance typed to the interface and not an implementation type.
		/// </summary>
		void ITelemetryManifestMatch.ValidateItself()
		{
		}
	}
}
