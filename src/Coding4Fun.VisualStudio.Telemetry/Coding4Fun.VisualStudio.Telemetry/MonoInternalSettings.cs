using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Internal settings implementation for Mono. Mostly this class just delegates behavour to the default windows implementation
	/// but it overrides the channel settings and Sqm policy which is not supported
	/// </summary>
	internal sealed class MonoInternalSettings : InternalSettingsBase
	{
		internal const string TelemetryUserDirKeyPath = "VSTelemetry";

		private const int ChannelExplicitlyEnabled = 1;

		private const int ChannelExplicitlyDisabled = 0;

		private JObject channelSettingsJson;

		public MonoInternalSettings(IDiagnosticTelemetry diagnosticTelemetry, IRegistryTools registryTools)
			: base(diagnosticTelemetry, registryTools)
		{
			LoadChannelSettings();
		}

		public override ChannelInternalSetting GetChannelSettings(string channelId)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(channelId, "channelId");
			int num = -1;
			try
			{
				JToken val = default(JToken);
				if (channelSettingsJson != null && channelSettingsJson.TryGetValue(channelId, StringComparison.OrdinalIgnoreCase, out val))
				{
					string a = ((object)val).ToString();
					if (a == "enabled")
					{
						num = 1;
					}
					if (a == "disabled")
					{
						num = 0;
					}
				}
			}
			catch
			{
				num = -1;
			}
			switch (num)
			{
			case 1:
				return ChannelInternalSetting.ExplicitlyEnabled;
			case 0:
				return ChannelInternalSetting.ExplicitlyDisabled;
			default:
				return base.GetChannelSettings(channelId);
			}
		}

		private void LoadChannelSettings()
		{
			try
			{
				string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "VSTelemetry", "channels.json");
				if (File.Exists(path))
				{
					channelSettingsJson = (JsonConvert.DeserializeObject(File.ReadAllText(path)) as JObject);
				}
			}
			catch
			{
				CoreEventSource.Log.LogError("Could not deserialize channel settings json");
				channelSettingsJson = null;
			}
		}
	}
}
