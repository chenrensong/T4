using Newtonsoft.Json.Linq;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class JsonTelemetryManifestActionConverter : JsonCreationConverter<ITelemetryManifestAction>
	{
		internal override ITelemetryManifestAction Create(Type objectType, JObject jsonObject)
		{
			if (FieldExists("excludeForChannels", jsonObject))
			{
				return new TelemetryManifestActionExclude();
			}
			if (FieldExists("route", jsonObject))
			{
				return new TelemetryManifestActionRoute();
			}
			if (FieldExists("optOutIncludeEvents", jsonObject))
			{
				return new TelemetryManifestActionOptOutIncludeEvents();
			}
			if (FieldExists("optOutExcludeEvents", jsonObject))
			{
				return new TelemetryManifestActionOptOutExcludeEvents();
			}
			if (FieldExists("optOutIncludeProperties", jsonObject))
			{
				return new TelemetryManifestActionOptOutIncludeProperties();
			}
			if (FieldExists("optOutExcludeProperties", jsonObject))
			{
				return new TelemetryManifestActionOptOutExcludeProperties();
			}
			if (FieldExists("throttle", jsonObject))
			{
				return new TelemetryManifestActionThrottle();
			}
			if (FieldExists("doNotThrottle", jsonObject))
			{
				return new TelemetryManifestActionDoNotThrottle();
			}
			if (FieldExists("piiProperties", jsonObject))
			{
				return new TelemetryManifestActionPii();
			}
			return new TelemetryManifestInvalidAction();
		}
	}
}
