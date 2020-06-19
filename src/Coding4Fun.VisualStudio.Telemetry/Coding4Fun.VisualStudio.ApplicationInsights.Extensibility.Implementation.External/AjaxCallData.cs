using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_AjaxCallData")]
	internal class AjaxCallData : PageViewData
	{
		public string ajaxUrl
		{
			get;
			set;
		}

		public double requestSize
		{
			get;
			set;
		}

		public double responseSize
		{
			get;
			set;
		}

		public string timeToFirstByte
		{
			get;
			set;
		}

		public string timeToLastByte
		{
			get;
			set;
		}

		public string callbackDuration
		{
			get;
			set;
		}

		public string responseCode
		{
			get;
			set;
		}

		public bool success
		{
			get;
			set;
		}

		public AjaxCallData()
			: this("AI.AjaxCallData", "AjaxCallData")
		{
		}

		protected AjaxCallData(string fullName, string name)
		{
			ajaxUrl = string.Empty;
			timeToFirstByte = string.Empty;
			timeToLastByte = string.Empty;
			callbackDuration = string.Empty;
			responseCode = string.Empty;
		}
	}
}
