using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_PageViewPerfData")]
	internal class PageViewPerfData : PageViewData
	{
		public string perfTotal
		{
			get;
			set;
		}

		public string networkConnect
		{
			get;
			set;
		}

		public string sentRequest
		{
			get;
			set;
		}

		public string receivedResponse
		{
			get;
			set;
		}

		public string domProcessing
		{
			get;
			set;
		}

		public PageViewPerfData()
			: this("AI.PageViewPerfData", "PageViewPerfData")
		{
		}

		protected PageViewPerfData(string fullName, string name)
		{
			perfTotal = string.Empty;
			networkConnect = string.Empty;
			sentRequest = string.Empty;
			receivedResponse = string.Empty;
			domProcessing = string.Empty;
		}
	}
}
