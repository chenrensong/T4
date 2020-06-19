namespace Coding4Fun.VisualStudio.ApplicationInsights
{
	internal class Constants
	{
		internal const string TelemetryServiceEndpoint = "https://dc.services.visualstudio.com/v2/track";

		internal const string TelemetryNamePrefix = "Coding4Fun.ApplicationInsights.";

		internal const string DevModeTelemetryNamePrefix = "Coding4Fun.ApplicationInsights.Dev.";

		internal const string TelemetryGroup = "{0d943590-b235-5bdb-f854-89520f32fc0b}";

		internal const string DevModeTelemetryGroup = "{ba84f32b-8af2-5006-f147-5030cdd7f22d}";

		internal const string EventSourceGroupTraitKey = "ETW_GROUP";

		internal const int MaxExceptionCountToSave = 10;
	}
}
