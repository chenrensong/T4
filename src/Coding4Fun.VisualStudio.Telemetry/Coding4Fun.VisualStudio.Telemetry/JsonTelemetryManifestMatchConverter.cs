using Coding4Fun.VisualStudio.Telemetry.JsonHelpers;
using Newtonsoft.Json.Linq;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class JsonTelemetryManifestMatchConverter : JsonCreationConverter<ITelemetryManifestMatch>
	{
		internal override ITelemetryManifestMatch Create(Type objectType, JObject jsonObject)
		{
			return jsonObject.CreateTelemetryManifestMatch();
		}
	}
}
