using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class WindowsInternalSettings : InternalSettingsBase
	{
		private const string FaultEventWatsonSampleRateRegKeyName = "FaultEventWatsonSampleRate";

		private const string FaultEventMaximumWatsonReportsPerSessionRegKeyName = "FaultEventMaximumWatsonReportsPerSession";

		private const string FaultEventMinimumSecondsBetweenWatsonReportsRegKeyName = "FaultEventMinimumSecondsBetweenWatsonReports";

		private const string GlobalPolicySqmClientRegistryPath = "Software\\Policies\\Microsoft\\SQMClient";

		private const string MsftInternalRegistryKeyName = "MSFTInternal";

		private const string EventTagTelemetryRegKeyName = "EventTag";

		public WindowsInternalSettings(IDiagnosticTelemetry diagnosticTelemetry, IRegistryTools registryTools)
			: base(diagnosticTelemetry, registryTools)
		{
			string userEventTag = GetUserEventTag();
			if (!string.IsNullOrEmpty(userEventTag))
			{
				diagnosticTelemetry.LogRegistrySettings("EventTag", userEventTag);
			}
		}

		/// <summary>
		/// Get the sample rate for Fault Events from registry
		/// useful for testing
		/// Reg add HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\Telemetry /v FaultEventWatsonSampleRate /t REG_DWORD /d 100 /f
		/// </summary>
		/// <returns></returns>
		public override int FaultEventWatsonSamplePercent()
		{
			int result = 0;
			TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Microsoft\\VisualStudio\\Telemetry", "FaultEventWatsonSampleRate", (object)10), out result);
			return result;
		}

		/// <summary>
		/// Get the default # of Watson reports per session from registry
		/// Reg add HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\Telemetry /v FaultEventMaximumWatsonReportsPerSession /t REG_DWORD /d 100 /f
		/// </summary>
		/// <returns></returns>
		public override int FaultEventMaximumWatsonReportsPerSession()
		{
			int result = 0;
			TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Microsoft\\VisualStudio\\Telemetry", "FaultEventMaximumWatsonReportsPerSession", (object)10), out result);
			return result;
		}

		/// <summary>
		/// Get the default # of seconds between Watson reports from registry
		/// Reg add HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\Telemetry /v FaultEventMinimumSecondsBetweenWatsonReports /t REG_DWORD /d 3600 /f
		/// </summary>
		/// <returns></returns>
		public override int FaultEventMinimumSecondsBetweenWatsonReports()
		{
			int result = 3600;
			TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Microsoft\\VisualStudio\\Telemetry", "FaultEventMinimumSecondsBetweenWatsonReports", (object)3600), out result);
			return result;
		}

		/// <summary>
		/// The registry can have an EventTag, such as "PerfLab":
		/// reg add HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\Telemetry /v EventTag /t reg_sz /d PerfLab
		/// this will be added to the Vs/TelemetryApi/Session/Initialized event with a property name "VS.TelemetryApi.RegistrySettings.EventTag", value "PerfLab"
		/// This tag can be changed without binary rebuild/re-release: the user can be instructed to set the value in the registry
		/// This EventTag can be used to group users, e.g. a select group of internal dogfood users, compare lab data to real world.
		/// </summary>
		/// <returns>a string. If empty, returns "null"</returns>
		public string GetUserEventTag()
		{
			return registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Microsoft\\VisualStudio\\Telemetry", "EventTag", (object)"null") as string;
		}
	}
}
