using Newtonsoft.Json;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchValueEndsWith : ITelemetryManifestMatchValue
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string EndsWith
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
			return valueToCompare?.ToString().EndsWith(EndsWith, StringComparison.OrdinalIgnoreCase) ?? false;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (string.IsNullOrEmpty(EndsWith))
			{
				throw new TelemetryManifestValidationException("'endsWith' can't be null or empty");
			}
		}
	}
}
