using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates information describing an Application Insights component.
	/// </summary>
	/// <remarks>
	/// This class matches the "Application" schema concept. We are intentionally calling it "Component" for consistency
	/// with terminology used by our portal and services and to encourage standardization of terminology within our
	/// organization. Once a consensus is reached, we will change type and property names to match.
	/// </remarks>
	public sealed class ComponentContext : IJsonSerializable
	{
		private readonly IDictionary<string, string> tags;

		/// <summary>
		/// Gets or sets the application version.
		/// </summary>
		public string Version
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.ApplicationVersion);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.ApplicationVersion, value);
			}
		}

		internal ComponentContext(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("version", Version);
			writer.WriteEndObject();
		}
	}
}
