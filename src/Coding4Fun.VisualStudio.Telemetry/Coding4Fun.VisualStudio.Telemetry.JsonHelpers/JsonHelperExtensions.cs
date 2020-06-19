using Newtonsoft.Json.Linq;

namespace Coding4Fun.VisualStudio.Telemetry.JsonHelpers
{
	internal static class JsonHelperExtensions
	{
		public static bool FieldExists(this JObject jObject, string fieldName)
		{
			if (jObject != null)
			{
				return jObject[fieldName] != null;
			}
			return false;
		}

		public static ITelemetryManifestMatch CreateTelemetryManifestMatch(this JObject jObject)
		{
			if (jObject.FieldExists("event"))
			{
				return new TelemetryManifestMatchEventName();
			}
			if (jObject.FieldExists("property"))
			{
				return new TelemetryManifestMatchPropertyValue();
			}
			if (jObject.FieldExists("and"))
			{
				return new TelemetryManifestMatchAnd();
			}
			if (jObject.FieldExists("or"))
			{
				return new TelemetryManifestMatchOr();
			}
			if (jObject.FieldExists("not"))
			{
				return new TelemetryManifestMatchNot();
			}
			if (jObject.FieldExists("samplingRate"))
			{
				return new TelemetryManifestMatchSampling();
			}
			return new TelemetryManifestInvalidMatchItem();
		}
	}
}
