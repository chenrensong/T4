using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Invalid match value item is necessary for specify invalid (unrecognized)
	/// match value item on JSON deserialization stage.
	/// During rule validation stage this item throws an exception
	/// and whole rule will be removed from the rule list.
	/// It allows us to partitially parse manifest files with new matching format.
	/// </summary>
	internal class TelemetryManifestInvalidMatchValueItem : ITelemetryManifestMatchValue
	{
		/// <summary>
		/// Check, whether passed value is match condition
		/// </summary>
		/// <param name="valueToCompare"></param>
		/// <returns></returns>
		public bool IsMatch(object valueToCompare)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			throw new TelemetryManifestValidationException("invalid match value item");
		}
	}
}
