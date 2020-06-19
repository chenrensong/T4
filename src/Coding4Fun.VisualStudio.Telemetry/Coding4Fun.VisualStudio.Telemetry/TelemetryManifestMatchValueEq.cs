using Newtonsoft.Json;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchValueEq : ITelemetryManifestMatchValue
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string Eq
		{
			get;
			set;
		}

		/// <summary>
		/// Check, whether passed value is match condition
		/// </summary>
		/// <param name="valueToCompare"></param>
		/// <returns></returns>
		public bool IsMatch(object valueToCompare)
		{
			if (valueToCompare == null)
			{
				return false;
			}
			return string.Compare(Eq, valueToCompare.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (Eq == null)
			{
				throw new TelemetryManifestValidationException("'eq' can't be null");
			}
		}
	}
}
