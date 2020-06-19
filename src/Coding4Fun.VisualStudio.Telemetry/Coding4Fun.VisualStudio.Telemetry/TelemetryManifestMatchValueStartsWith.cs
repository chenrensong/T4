using Newtonsoft.Json;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchValueStartsWith : ITelemetryManifestMatchValue
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string StartsWith
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
			return valueToCompare?.ToString().StartsWith(StartsWith, StringComparison.OrdinalIgnoreCase) ?? false;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (string.IsNullOrEmpty(StartsWith))
			{
				throw new TelemetryManifestValidationException("'startsWith' can't be null or empty");
			}
		}
	}
}
