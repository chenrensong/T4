using Newtonsoft.Json.Linq;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class JsonTelemetryManifestRouteArgsConverter : JsonCreationConverter<ITelemetryManifestRouteArgs>
	{
		internal override ITelemetryManifestRouteArgs Create(Type objectType, JObject jsonObject)
		{
			if (FieldExists("datapointId", jsonObject))
			{
				return new TelemetryManifestLegacyDatapointRouteArgs();
			}
			if (FieldExists("streamId", jsonObject))
			{
				return new TelemetryManifestLegacyStreamRouteArgs();
			}
			if (FieldExists("propertyName", jsonObject))
			{
				return new TelemetryManifestLegacyStreamPropertyRouteArgs();
			}
			return new TelemetryManifestInvalidRouteArgs();
		}
	}
}
