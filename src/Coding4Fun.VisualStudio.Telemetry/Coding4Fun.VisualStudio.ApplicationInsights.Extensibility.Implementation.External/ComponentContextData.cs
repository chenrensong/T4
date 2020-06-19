using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Encapsulates information describing an Application Insights component.
	/// </summary>
	/// <remarks>
	/// This class matches the "Application" schema concept. We are intentionally calling it "Component" for consistency
	/// with terminology used by our portal and services and to encourage standardization of terminology within our
	/// organization. Once a consensus is reached, we will change type and property names to match.
	/// </remarks>
	internal sealed class ComponentContextData
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

		/// <summary>
		/// Gets or sets the application version.
		/// </summary>
		public string Build
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.ApplicationBuild);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.ApplicationBuild, value);
			}
		}

		internal ComponentContextData(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		internal void SetDefaults(ComponentContextData source)
		{
			tags.InitializeTagValue(ContextTagKeys.Keys.ApplicationVersion, source.Version);
			tags.InitializeTagValue(ContextTagKeys.Keys.ApplicationBuild, source.Build);
		}
	}
}
