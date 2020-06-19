using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Encapsulates telemetry location information.
	/// </summary>
	internal sealed class LocationContextData
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
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.LocationIp, value);
			}
		}

		internal LocationContextData(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}
	}
}
