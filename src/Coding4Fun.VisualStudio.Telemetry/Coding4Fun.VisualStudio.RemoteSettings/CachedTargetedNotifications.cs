using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class CachedTargetedNotifications
	{
		/// <summary>
		/// Gets or sets a dictionary of CategoryId -&gt; CachedActionCategoryTime
		/// </summary>
		public IDictionary<string, CachedActionCategoryTime> Categories
		{
			get;
			set;
		} = new Dictionary<string, CachedActionCategoryTime>();


		/// <summary>
		/// Gets or sets a dictionary of RuleId -&gt; CachedActionResponseTime
		/// </summary>
		public IDictionary<string, CachedActionResponseTime> Actions
		{
			get;
			set;
		} = new Dictionary<string, CachedActionResponseTime>();

	}
}
