using Newtonsoft.Json;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMachineIdentityConfig
	{
		public class SmaRule
		{
			public enum TargetType
			{
				Description,
				ConnectionType,
				MACAddress
			}

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public uint Position
			{
				get;
				set;
			}

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public TargetType Target
			{
				get;
				set;
			}

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public string Regex
			{
				get;
				set;
			}

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public int TierAdjustment
			{
				get;
				set;
			}

			[JsonProperty(/*Could not decode attribute arguments.*/)]
			public bool StopProcessing
			{
				get;
				set;
			}
		}

		private const string DefaultMinValidConfigVersion = "IDv15.00000";

		internal const string DefaultConfigVersion = "IDv15.00001";

		public static TelemetryManifestMachineIdentityConfig DefaultConfig
		{
			get
			{
				TelemetryManifestMachineIdentityConfig telemetryManifestMachineIdentityConfig = new TelemetryManifestMachineIdentityConfig();
				telemetryManifestMachineIdentityConfig.ConfigVersion = "IDv15.00001";
				telemetryManifestMachineIdentityConfig.HardwareIdComponents = new string[3]
				{
					"BiosUUID",
					"BiosSerialNumber",
					"PersistedSelectedMACAddress"
				};
				telemetryManifestMachineIdentityConfig.InvalidateOnPrimaryIdChange = false;
				telemetryManifestMachineIdentityConfig.MinValidConfigVersion = "IDv15.00000";
				telemetryManifestMachineIdentityConfig.SendValuesEvent = true;
				telemetryManifestMachineIdentityConfig.SmaRules = new List<SmaRule>
				{
					new SmaRule
					{
						Position = 0u,
						Target = SmaRule.TargetType.Description,
						StopProcessing = true,
						Regex = "( Loopback )|(TAP-?)|(TEST)|(VPN)|(Remote )",
						TierAdjustment = 10000
					},
					new SmaRule
					{
						Position = 1u,
						Target = SmaRule.TargetType.Description,
						StopProcessing = true,
						Regex = "^(Intel|Marvell|Realtek)",
						TierAdjustment = -100
					},
					new SmaRule
					{
						Position = 2u,
						Target = SmaRule.TargetType.Description,
						StopProcessing = true,
						Regex = "^(Qualcomm Atheros|Killer |Surface Ethernet Adapter|Broadcom |Dell |TP-Link|Apple |D-Link)",
						TierAdjustment = -50
					},
					new SmaRule
					{
						Position = 3u,
						Target = SmaRule.TargetType.Description,
						StopProcessing = false,
						Regex = "([V|v]irt(IO|u(al|ální|eller)))|(仮)|(PdaNet )|(vmxnet)|(IPVanish)|(RNDIS )",
						TierAdjustment = 300
					},
					new SmaRule
					{
						Position = 4u,
						Target = SmaRule.TargetType.Description,
						StopProcessing = false,
						Regex = "(Microsoft Hyper-V Network Adapter|USB |Targus)",
						TierAdjustment = 200
					}
				};
				return telemetryManifestMachineIdentityConfig;
			}
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string[] HardwareIdComponents
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string ConfigVersion
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public List<SmaRule> SmaRules
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public bool SendValuesEvent
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string MinValidConfigVersion
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public bool InvalidateOnPrimaryIdChange
		{
			get;
			set;
		}
	}
}
