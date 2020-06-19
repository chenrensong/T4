using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestLegacyDatapointRouteArgs : ITelemetryManifestRouteArgs
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public uint DatapointId
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public LegacyDatapointType DataType
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string ParameterName
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

		public void Validate()
		{
			if (DatapointId == 0)
			{
				throw new TelemetryManifestValidationException("datapointId should not be 0");
			}
			if (StringExtensions.IsNullOrWhiteSpace(ParameterName))
			{
				throw new TelemetryManifestValidationException("parameter name must not be empty");
			}
		}
	}
}
