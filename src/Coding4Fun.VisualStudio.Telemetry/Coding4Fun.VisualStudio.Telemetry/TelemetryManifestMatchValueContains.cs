using Newtonsoft.Json;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchValueContains : ITelemetryManifestMatchValue
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string Contains
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
			return valueToCompare.ToString().IndexOf(Contains, StringComparison.OrdinalIgnoreCase) != -1;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (string.IsNullOrEmpty(Contains))
			{
				throw new TelemetryManifestValidationException("'contains' can't be null or empty");
			}
		}
	}
}
