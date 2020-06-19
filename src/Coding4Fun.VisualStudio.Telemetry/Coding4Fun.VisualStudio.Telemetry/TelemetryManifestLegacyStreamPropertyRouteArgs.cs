using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestLegacyStreamPropertyRouteArgs : ITelemetryManifestRouteArgs
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public LegacyDatapointType DataType
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string PropertyName
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "truncationRule")]
		public LegacyStringTruncationRule TruncationRule
		{
			get;
			set;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (StringExtensions.IsNullOrWhiteSpace(PropertyName))
			{
				throw new TelemetryManifestValidationException("property name must not be empty");
			}
		}
	}
}
