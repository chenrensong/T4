using Coding4Fun.VisualStudio.ApplicationInsights.Channel;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal static class TelemetryItemExtensions
	{
		internal static string GetTelemetryFullName(this ITelemetry item, string envelopeName)
		{
			return "Coding4Fun.ApplicationInsights." + item.Context.InstrumentationKey + "|" + envelopeName;
		}
	}
}
