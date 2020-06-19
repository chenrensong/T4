using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates Internal information.
	/// </summary>
	public sealed class InternalContext : IJsonSerializable
	{
		private readonly IDictionary<string, string> tags;

		/// <summary>
		/// Gets or sets application insights SDK version.
		/// </summary>
		public string SdkVersion
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.InternalSdkVersion);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.InternalSdkVersion, value);
			}
		}

		/// <summary>
		/// Gets or sets application insights agent version.
		/// </summary>
		public string AgentVersion
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.InternalAgentVersion);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.InternalAgentVersion, value);
			}
		}

		internal InternalContext(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("sdkVersion", SdkVersion);
			writer.WriteProperty("agentVersion", AgentVersion);
			writer.WriteEndObject();
		}
	}
}
