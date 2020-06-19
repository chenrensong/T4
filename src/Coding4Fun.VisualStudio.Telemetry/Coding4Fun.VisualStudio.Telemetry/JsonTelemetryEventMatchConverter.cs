using Coding4Fun.VisualStudio.Telemetry.JsonHelpers;
using Newtonsoft.Json.Linq;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class JsonTelemetryEventMatchConverter : JsonCreationConverter<ITelemetryEventMatch>
	{
		internal override ITelemetryEventMatch Create(Type objectType, JObject jsonObject)
		{
			return jsonObject.CreateTelemetryManifestMatch();
		}
	}
}
