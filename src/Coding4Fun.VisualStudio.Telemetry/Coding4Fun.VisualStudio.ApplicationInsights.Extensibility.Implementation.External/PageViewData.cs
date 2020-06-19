using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_PageViewData")]
	internal class PageViewData : EventData
	{
		public string url
		{
			get;
			set;
		}

		public string duration
		{
			get;
			set;
		}

		public PageViewData()
			: this("AI.PageViewData", "PageViewData")
		{
		}

		protected PageViewData(string fullName, string name)
		{
			url = string.Empty;
			duration = string.Empty;
		}
	}
}
