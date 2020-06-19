using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchOr : ITelemetryManifestMatch, ITelemetryEventMatch
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<ITelemetryManifestMatch> Or
		{
			get;
			set;
		}

		public IEnumerable<ITelemetryManifestMatch> GetChildren()
		{
			return Or;
		}

		/// <summary>
		/// Check, whether does event match conditions
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <returns></returns>
		public bool IsEventMatch(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			foreach (ITelemetryManifestMatch item in Or)
			{
				if (item.IsEventMatch(telemetryEvent))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Validates only 'this' class but not children. Explicity specify interface so that this method
		/// can only be called an instance typed to the interface and not an implementation type.
		/// </summary>
		void ITelemetryManifestMatch.ValidateItself()
		{
			if (!Or.Any())
			{
				throw new TelemetryManifestValidationException("there are no operands in the 'or' clause");
			}
		}
	}
}
