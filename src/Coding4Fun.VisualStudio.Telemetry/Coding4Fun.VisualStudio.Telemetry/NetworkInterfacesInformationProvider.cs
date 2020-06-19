using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class obtains and provides the network interface information, and includes customized prioritication logic to improve the selection of a MAC address identifier for the local machine.
	/// </summary>
	internal class NetworkInterfacesInformationProvider : INetworkInterfacesInformationProvider
	{
		private struct NetworkInterfaceInformation
		{
			public List<NetworkInterfaceCardInformation> PrioritizedNetworkInterfaces;

			public string SelectedMACAddress;
		}

		private static Lazy<NetworkInterfaceInformation> networkInterfacesInformation;

		private const int NetworkInterfaceSelection_Tier1 = 100;

		private const int NetworkInterfaceSelection_Tier2 = 200;

		private const int NetworkInterfaceSelection_Tier3 = 300;

		private const int NetworkInterfaceSelection_ExcludedTier = 10000;

		private const int NetworkInterfaceSelection_Prioritization = -100;

		private const int NetworkInterfaceSelection_Minor_Prioritization = -50;

		private const int NetworkInterfaceSelection_Minor_Deprioritization = 200;

		private const int NetworkInterfaceSelection_Deprioritization = 300;

		private static readonly Dictionary<NetworkInterfaceType, int> NetworkInterfaceTypeIdPriorities = new Dictionary<NetworkInterfaceType, int>
		{
			{
				NetworkInterfaceType.Wireless80211,
				100
			},
			{
				NetworkInterfaceType.Ethernet,
				200
			},
			{
				NetworkInterfaceType.GigabitEthernet,
				200
			},
			{
				NetworkInterfaceType.FastEthernetT,
				200
			},
			{
				NetworkInterfaceType.Ethernet3Megabit,
				200
			},
			{
				NetworkInterfaceType.FastEthernetFx,
				200
			},
			{
				NetworkInterfaceType.Fddi,
				200
			},
			{
				NetworkInterfaceType.TokenRing,
				300
			},
			{
				NetworkInterfaceType.Isdn,
				300
			},
			{
				NetworkInterfaceType.BasicIsdn,
				300
			},
			{
				NetworkInterfaceType.PrimaryIsdn,
				300
			},
			{
				NetworkInterfaceType.RateAdaptDsl,
				300
			},
			{
				NetworkInterfaceType.SymmetricDsl,
				300
			},
			{
				NetworkInterfaceType.VeryHighSpeedDsl,
				300
			},
			{
				NetworkInterfaceType.MultiRateSymmetricDsl,
				300
			}
		};

		private static readonly Regex ExcludedNetworkInterfaceRegex = new Regex("( Loopback )|(TAP-?)|(TEST)|(VPN)|(Remote )", RegexOptions.CultureInvariant);

		private static readonly Regex MinorDeprioritizedNetworkInterfaceDescriptionRegex = new Regex("(Microsoft Hyper-V Network Adapter|USB |Targus)", RegexOptions.CultureInvariant);

		private static readonly Regex DeprioritizedNetworkInterfaceDescriptionRegex = new Regex("([V|v]irt(IO|u(al|ální|eller)))|(仮)|(PdaNet )|(vmxnet)|(IPVanish)|(RNDIS )", RegexOptions.CultureInvariant);

		private static readonly Regex PrioritizedPhysicalManufacturersRegex = new Regex("^(Intel|Marvell|Realtek)", RegexOptions.CultureInvariant);

		private static readonly Regex KnownPhysicalManufacturersRegex = new Regex("^(Qualcomm Atheros|Killer |Surface Ethernet Adapter|Broadcom |Dell |TP-Link|Apple |D-Link)", RegexOptions.CultureInvariant);

		public string SelectedMACAddress => networkInterfacesInformation.Value.SelectedMACAddress;

		/// <summary>
		/// Gets the priorized list of network interfaces.
		/// </summary>
		public List<NetworkInterfaceCardInformation> PrioritizedNetworkInterfaces => networkInterfacesInformation.Value.PrioritizedNetworkInterfaces;

		private List<TelemetryManifestMachineIdentityConfig.SmaRule> SelectedMacAddressRules
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the List of network interfaces from the local machine
		/// </summary>
		/// <exception cref="T:System.Net.NetworkInformation.NetworkInformationException">Exception occurs if the the call fails</exception>
		/// <returns>The information about all Network Interfaces</returns>
		private Func<IEnumerable<NetworkInterfaceCardInformation>> GetAllNetworkInterfaceCardInformation
		{
			get;
		} = () => from nic in NetworkInterface.GetAllNetworkInterfaces()
			select new NetworkInterfaceCardInformation
			{
				Description = nic.Description,
				MacAddress = nic.GetPhysicalAddress().ToString(),
				NetworkInterfaceType = nic.NetworkInterfaceType,
				OperationalStatus = nic.OperationalStatus
			};


		/// <summary>
		/// Creates an instnace of the NetworkInterfacesInformationProvider.
		/// </summary>
		/// <param name="rules"></param>
		/// <param name="getAllNetworkInterfaceCardInformation">Opational paramter to alter the source of the network interface list for the local machine.</param>
		public NetworkInterfacesInformationProvider(List<TelemetryManifestMachineIdentityConfig.SmaRule> rules, Func<IEnumerable<NetworkInterfaceCardInformation>> getAllNetworkInterfaceCardInformation = null)
		{
			SelectedMacAddressRules = rules;
			GetAllNetworkInterfaceCardInformation = (getAllNetworkInterfaceCardInformation ?? GetAllNetworkInterfaceCardInformation);
			networkInterfacesInformation = new Lazy<NetworkInterfaceInformation>(InitializeNetworkInterfacesInformation);
		}

		private NetworkInterfaceInformation InitializeNetworkInterfacesInformation()
		{
			List<NetworkInterfaceCardInformation> list = (from nic in GetAllNetworkInterfaceCardInformation().Select(delegate(NetworkInterfaceCardInformation nic)
				{
					nic.SelectionTier = GetSelectionTeir(nic);
					return nic;
				})
				orderby nic.SelectionTier, nic.MacAddress
				select nic).ToList();
			int rank = 0;
			list.ForEach(delegate(NetworkInterfaceCardInformation nic)
			{
				nic.SelectionRank = rank++;
			});
			NetworkInterfaceInformation result = default(NetworkInterfaceInformation);
			result.PrioritizedNetworkInterfaces = list;
			result.SelectedMACAddress = list.FirstOrDefault()?.MacAddress;
			return result;
		}

		private int GetSelectionTeir(NetworkInterfaceCardInformation nic)
		{
			if (!NetworkInterfaceTypeIdPriorities.TryGetValue(nic.NetworkInterfaceType, out int value))
			{
				return 10000;
			}
			foreach (TelemetryManifestMachineIdentityConfig.SmaRule item in SelectedMacAddressRules.OrderBy((TelemetryManifestMachineIdentityConfig.SmaRule r) => r.Position))
			{
				string input;
				switch (item.Target)
				{
				case TelemetryManifestMachineIdentityConfig.SmaRule.TargetType.Description:
					input = nic.Description;
					break;
				case TelemetryManifestMachineIdentityConfig.SmaRule.TargetType.ConnectionType:
					input = nic.NetworkInterfaceType.ToString();
					break;
				case TelemetryManifestMachineIdentityConfig.SmaRule.TargetType.MACAddress:
					input = nic.MacAddress;
					break;
				default:
					continue;
				}
				if (Regex.IsMatch(input, item.Regex, RegexOptions.CultureInvariant))
				{
					value += item.TierAdjustment;
					if (item.StopProcessing)
					{
						return value;
					}
				}
			}
			return value;
		}
	}
}
