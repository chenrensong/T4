using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface INetworkInterfacesInformationProvider
	{
		/// <summary>
		/// Gets the Selected MAC Address of the local machine from customized network interface prioritization logic.
		/// </summary>
		string SelectedMACAddress
		{
			get;
		}

		/// <summary>
		/// Gets a list of the local machine's Network interfaces ordered by customized prioritization logic.
		/// </summary>
		List<NetworkInterfaceCardInformation> PrioritizedNetworkInterfaces
		{
			get;
		}
	}
}
