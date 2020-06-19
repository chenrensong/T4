using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestLegacyStreamRouteArgs : ITelemetryManifestRouteArgs
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public uint StreamId
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<ITelemetryManifestRouteArgs> Properties
		{
			get;
			set;
		}

		public void Validate()
		{
			if (StreamId == 0)
			{
				throw new TelemetryManifestValidationException("streamId should not be 0");
			}
			if (Properties == null)
			{
				throw new TelemetryManifestValidationException("properties are null");
			}
			if (!Properties.Any())
			{
				throw new TelemetryManifestValidationException("there are no properties");
			}
			if (Properties.Any((ITelemetryManifestRouteArgs x) => x.GetType() != typeof(TelemetryManifestLegacyStreamPropertyRouteArgs)))
			{
				throw new TelemetryManifestValidationException("properties contain incorrect children");
			}
			foreach (ITelemetryManifestRouteArgs property in Properties)
			{
				property.Validate();
			}
		}
	}
}
