using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates telemetry location information.
	/// </summary>
	public sealed class LocationContext : IJsonSerializable
	{
		private readonly IDictionary<string, string> tags;

		/// <summary>
		/// Gets or sets the location IP.
		/// </summary>
		public string Ip
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.LocationIp);
			}
			set
			{
				if (value != null && IsIpV4(value))
				{
					tags.SetStringValueOrRemove(ContextTagKeys.Keys.LocationIp, value);
				}
			}
		}

		internal LocationContext(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("ip", Ip);
			writer.WriteEndObject();
		}

		private bool IsIpV4(string ip)
		{
			if (ip.Length > 15 || ip.Length < 7)
			{
				return false;
			}
			if (ip.Cast<char>().Any((char c) => (c < '0' || c > '9') && c != '.'))
			{
				return false;
			}
			string[] array = ip.Split('.');
			if (array.Length != 4)
			{
				return false;
			}
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (!byte.TryParse(array2[i], out byte _))
				{
					return false;
				}
			}
			return true;
		}
	}
}
