namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryManifestMatchValue
	{
		/// <summary>
		/// Check, whether passed value is match condition
		/// </summary>
		/// <param name="valueToCompare"></param>
		/// <returns></returns>
		bool IsMatch(object valueToCompare);

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		void Validate();
	}
}
