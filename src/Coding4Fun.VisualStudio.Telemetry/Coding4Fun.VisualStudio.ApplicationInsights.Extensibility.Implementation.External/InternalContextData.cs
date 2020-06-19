using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Internal context type shared between SDK and DP.
	/// </summary>
	internal sealed class InternalContextData
	{
		private readonly IDictionary<string, string> tags;

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

		internal InternalContextData(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}
	}
}
