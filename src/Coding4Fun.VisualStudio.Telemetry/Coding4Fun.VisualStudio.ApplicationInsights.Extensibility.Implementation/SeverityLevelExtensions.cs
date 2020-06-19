using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal static class SeverityLevelExtensions
	{
		public static Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel? TranslateSeverityLevel(this Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel? sdkSeverityLevel)
		{
			if (!sdkSeverityLevel.HasValue)
			{
				return null;
			}
			switch (sdkSeverityLevel.Value)
			{
			case Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Critical:
				return Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Critical;
			case Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Error:
				return Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Error;
			case Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Warning:
				return Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Warning;
			case Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Information:
				return Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Information;
			default:
				return Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Verbose;
			}
		}

		public static Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel? TranslateSeverityLevel(this Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel? dataPlatformSeverityLevel)
		{
			if (!dataPlatformSeverityLevel.HasValue)
			{
				return null;
			}
			switch (dataPlatformSeverityLevel.Value)
			{
			case Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Critical:
				return Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Critical;
			case Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Error:
				return Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Error;
			case Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Warning:
				return Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Warning;
			case Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SeverityLevel.Information:
				return Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Information;
			default:
				return Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SeverityLevel.Verbose;
			}
		}
	}
}
