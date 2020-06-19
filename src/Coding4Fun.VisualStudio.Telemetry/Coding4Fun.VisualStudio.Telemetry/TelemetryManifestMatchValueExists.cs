using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Match event if property exists in the event.
	///
	/// Syntax
	/// {
	///     property : "VS.Core.SKU",
	///     value : {
	///         exists : true
	///     }
	/// }
	/// </summary>
	internal class TelemetryManifestMatchValueExists : ITelemetryManifestMatchValue
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public bool Exists
		{
			get;
			set;
		}

		/// <summary>
		/// Check, whether passed value is match condition
		/// If property is exist this method will be called.
		/// </summary>
		/// <param name="valueToCompare"></param>
		/// <returns></returns>
		public bool IsMatch(object valueToCompare)
		{
			return true;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (!Exists)
			{
				throw new TelemetryManifestValidationException("the only allowable value for 'exists' is true");
			}
		}
	}
}
