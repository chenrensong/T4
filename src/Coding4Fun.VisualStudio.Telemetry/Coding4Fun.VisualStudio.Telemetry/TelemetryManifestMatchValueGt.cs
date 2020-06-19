using Newtonsoft.Json;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchValueGt : ITelemetryManifestMatchValue
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public double Gt
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
			if (!TypeTools.IsNumericType(valueToCompare.GetType()))
			{
				return false;
			}
			return Convert.ToDouble(valueToCompare, null) > Gt;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (double.IsNaN(Gt) || double.IsInfinity(Gt))
			{
				throw new TelemetryManifestValidationException("'gt' must be valid double value");
			}
		}
	}
}
