using Newtonsoft.Json.Linq;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class JsonTelemetryManifestMatchValueConverter : JsonCreationConverter<ITelemetryManifestMatchValue>
	{
		internal override ITelemetryManifestMatchValue Create(Type objectType, JObject jsonObject)
		{
			if (FieldExists("eq", jsonObject))
			{
				return new TelemetryManifestMatchValueEq();
			}
			if (FieldExists("lt", jsonObject))
			{
				return new TelemetryManifestMatchValueLt();
			}
			if (FieldExists("gt", jsonObject))
			{
				return new TelemetryManifestMatchValueGt();
			}
			if (FieldExists("exists", jsonObject))
			{
				return new TelemetryManifestMatchValueExists();
			}
			if (FieldExists("startsWith", jsonObject))
			{
				return new TelemetryManifestMatchValueStartsWith();
			}
			if (FieldExists("endsWith", jsonObject))
			{
				return new TelemetryManifestMatchValueEndsWith();
			}
			if (FieldExists("contains", jsonObject))
			{
				return new TelemetryManifestMatchValueContains();
			}
			return new TelemetryManifestInvalidMatchValueItem();
		}
	}
}
