using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Globalization;
using System.Net.NetworkInformation;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class InternalSettingsBase : IInternalSettings
	{
		internal const string TelemetryUserRegKeyPath = "Software\\Coding4Fun\\VisualStudio\\Telemetry";

		internal const string CompletelyDisabledTelemetryRegKeyName = "TurnOffSwitch";

		internal const string LocalLoggerEnabledRegKeyName = "LocalLoggerEnabled";

		internal const int LocalLoggerEnabled = 1;

		internal const int CompletelyDisabledTelemetry = 1;

		private const string RegKeyChannelSettings = "\\Channels";

		private const int ChannelExplicitlyEnabled = 1;

		private const int ChannelExplicitlyDisabled = 0;

		private const string ForceExternalUserRegKeyName = "ForceExternalUser";

		private const int ForcedUserExternal = 1;

		private const string TestHostNameRegKeyName = "UseTestHostName";

		private const string TestAppIdRegKeyName = "UseTestAppId";

		protected readonly IRegistryTools registryTools;

		protected readonly IDiagnosticTelemetry diagnosticTelemetry;

		/// <summary>
		/// Main constructor that requests all required classes.
		/// </summary>
		/// <param name="diagnosticTelemetry"></param>
		/// <param name="registryTools"></param>
		public InternalSettingsBase(IDiagnosticTelemetry diagnosticTelemetry, IRegistryTools registryTools)
		{
			CodeContract.RequiresArgumentNotNull<IDiagnosticTelemetry>(diagnosticTelemetry, "diagnosticTelemetry");
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(registryTools, "registryTools");
			this.registryTools = registryTools;
			this.diagnosticTelemetry = diagnosticTelemetry;
		}

		/// <summary>
		/// Get internal settings for the channel specified by its ID.
		/// There are 3 states could be:
		/// - explicitly enabled
		/// - explicitly disabled
		/// - undefined (no settings available)
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		public virtual ChannelInternalSetting GetChannelSettings(string channelId)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(channelId, "channelId");
			if (!TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\Telemetry\\Channels", channelId, (object)(-1)), out int result))
			{
				result = -1;
			}
			ChannelInternalSetting channelInternalSetting = ChannelInternalSetting.Undefined;
			switch (result)
			{
			case 1:
				channelInternalSetting = ChannelInternalSetting.ExplicitlyEnabled;
				break;
			case 0:
				channelInternalSetting = ChannelInternalSetting.ExplicitlyDisabled;
				break;
			}
			if (channelInternalSetting != ChannelInternalSetting.Undefined)
			{
				diagnosticTelemetry.LogRegistrySettings(string.Format(CultureInfo.InvariantCulture, "Channel.{0}", new object[1]
				{
					channelId
				}), channelInternalSetting.ToString());
			}
			return channelInternalSetting;
		}

		/// <summary>
		/// Check whether user is forced set as external
		/// </summary>
		/// <returns></returns>
		public bool IsForcedUserExternal()
		{
			TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\Telemetry", "ForceExternalUser", (object)0), out int result);
			bool num = result == 1;
			if (num)
			{
				diagnosticTelemetry.LogRegistrySettings("ForcedExternalUser", "true");
			}
			return num;
		}

		/// <summary>
		/// Try to get test AppId settings from the registry.
		/// </summary>
		/// <param name="testAppId"></param>
		/// <returns></returns>
		public bool TryGetTestAppId(out uint testAppId)
		{
			TypeTools.TryConvertToUInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\Telemetry", "UseTestAppId", (object)0), out testAppId);
			if (testAppId != 0)
			{
				diagnosticTelemetry.LogRegistrySettings("TestAppId", testAppId.ToString(CultureInfo.InvariantCulture));
			}
			return testAppId != 0;
		}

		/// <summary>
		/// Try to get test hostName from the registry.
		/// </summary>
		/// <param name="testHostName"></param>
		/// <returns></returns>
		public bool TryGetTestHostName(out string testHostName)
		{
			testHostName = TypeTools.ConvertToString(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\Telemetry", "UseTestHostName", (object)null));
			bool num = !string.IsNullOrEmpty(testHostName);
			if (num)
			{
				diagnosticTelemetry.LogRegistrySettings("TestHostName", testHostName);
			}
			return num;
		}

		/// <summary>
		/// Returns the IP Global Config Domain Name or empty string in case of network exception.
		/// </summary>
		/// <returns></returns>
		public string GetIPGlobalConfigDomainName()
		{
			string result = string.Empty;
			try
			{
				result = IPGlobalProperties.GetIPGlobalProperties().DomainName;
				return result;
			}
			catch (NetworkInformationException)
			{
				return result;
			}
		}

		/// <summary>
		/// Check, whether telemetry completely disabled
		/// Reg add HKEY_CURRENT_USER\Software\Coding4Fun\VisualStudio\Telemetry /v TurnOffSwitch /t REG_DWORD /d 1 /f
		/// </summary>
		/// <returns></returns>
		public bool IsTelemetryDisabledCompletely()
		{
			TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\Telemetry", "TurnOffSwitch", (object)0), out int result);
			bool num = result == 1;
			if (num)
			{
				diagnosticTelemetry.LogRegistrySettings("CompletelyDisabledTelemetry", "true");
			}
			return num;
		}

		/// <summary>
		/// Check whether local logger is enabled
		/// </summary>
		/// <returns></returns>
		public bool IsLocalLoggerEnabled()
		{
			bool flag = false;
			if (Platform.IsWindows)
			{
				TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\Telemetry", "LocalLoggerEnabled", (object)0), out int result);
				flag = (result == 1);
			}
			if (flag)
			{
				diagnosticTelemetry.LogRegistrySettings("LocalLoggerEnabled", "true");
			}
			return flag;
		}

		/// <summary>
		/// Get the sample rate for Fault Events from registry
		/// useful for testing
		/// Reg add HKEY_CURRENT_USER\Software\Coding4Fun\VisualStudio\Telemetry /v FaultEventWatsonSampleRate /t REG_DWORD /d 100 /f
		/// </summary>
		/// <returns></returns>
		public virtual int FaultEventWatsonSamplePercent()
		{
			return 0;
		}

		/// <summary>
		/// Get the default # of Watson reports per session from registry
		/// Reg add HKEY_CURRENT_USER\Software\Coding4Fun\VisualStudio\Telemetry /v FaultEventMaximumWatsonReportsPerSession /t REG_DWORD /d 100 /f
		/// </summary>
		/// <returns></returns>
		public virtual int FaultEventMaximumWatsonReportsPerSession()
		{
			return 0;
		}

		/// <summary>
		/// Get the default # of seconds bewtween Watson reports from registry
		/// Reg add HKEY_CURRENT_USER\Software\Coding4Fun\VisualStudio\Telemetry /v FaultEventMinimumSecondsBetweenWatsonReports /t REG_DWORD /d 3600 /f
		/// </summary>
		/// <returns></returns>
		public virtual int FaultEventMinimumSecondsBetweenWatsonReports()
		{
			return 0;
		}
	}
}
