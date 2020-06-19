using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Net.NetworkInformation;

namespace Coding4Fun.VisualStudio.Telemetry
{
	[Serializable]
	internal class NetworkInterfaceCardInformation
	{
		/// <summary>
		/// The priorizied rank of the network interface
		/// </summary>
		public int SelectionRank;

		/// <summary>
		/// The MAC address of the network Interface
		/// </summary>
		[JsonIgnore]
		public string MacAddress;

		/// <summary>
		/// The selection tier of the network interface
		/// </summary>
		public int SelectionTier;

		/// <summary>
		/// The operational status of the network interface
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public OperationalStatus OperationalStatus;

		/// <summary>
		/// The network interface type
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public NetworkInterfaceType NetworkInterfaceType;

		/// <summary>
		/// The manufacturer's description of the network interface often appeneded by the Windows OS with a number to disambiguate between adapters of the same model.
		/// </summary>
		public string Description;

		/// <summary>
		/// Serializes this object to JSON omitting PII values
		/// </summary>
		/// <returns>The JSON serialized object</returns>
		public string Serialize()
		{
			return JsonConvert.SerializeObject((object)this);
		}
	}
}
