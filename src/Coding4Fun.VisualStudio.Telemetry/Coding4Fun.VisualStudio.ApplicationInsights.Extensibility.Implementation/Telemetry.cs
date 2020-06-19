using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using System.Globalization;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal static class Telemetry
	{
		public static void WriteEnvelopeProperties(this ITelemetry telemetry, IJsonWriter json)
		{
			json.WriteProperty("time", telemetry.Timestamp);
			json.WriteProperty("seq", telemetry.Sequence);
			((IJsonSerializable)telemetry.Context).Serialize(json);
		}

		public static void WriteTelemetryName(this ITelemetry telemetry, IJsonWriter json, string telemetryName)
		{
			bool result = false;
			ISupportProperties supportProperties = telemetry as ISupportProperties;
			if (supportProperties != null && supportProperties.Properties.TryGetValue("DeveloperMode", out string value))
			{
				bool.TryParse(value, out result);
			}
			string value2 = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", new object[3]
			{
				result ? "Coding4Fun.ApplicationInsights.Dev." : "Coding4Fun.ApplicationInsights.",
				NormalizeInstrumentationKey(telemetry.Context.InstrumentationKey),
				telemetryName
			});
			json.WriteProperty("name", value2);
		}

		/// <summary>
		/// Normalize instrumentation key by removing dashes ('-') and making string in the lowercase.
		/// In case no InstrumentationKey is available just return empty string.
		/// In case when InstrumentationKey is available return normalized key + dot ('.')
		/// as a separator between instrumentation key part and telemetry name part.
		/// </summary>
		/// <returns></returns>
		private static string NormalizeInstrumentationKey(string instrumentationKey)
		{
			if (instrumentationKey.IsNullOrWhiteSpace())
			{
				return string.Empty;
			}
			return instrumentationKey.Replace("-", string.Empty).ToLowerInvariant() + ".";
		}
	}
}
