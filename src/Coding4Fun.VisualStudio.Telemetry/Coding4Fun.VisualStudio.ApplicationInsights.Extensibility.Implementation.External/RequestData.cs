using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_RequestData")]
	internal class RequestData
	{
		public int ver
		{
			get;
			set;
		}

		public string id
		{
			get;
			set;
		}

		public string name
		{
			get;
			set;
		}

		public string startTime
		{
			get;
			set;
		}

		public string duration
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

		public string httpMethod
		{
			get;
			set;
		}

		public string url
		{
			get;
			set;
		}

		public IDictionary<string, string> properties
		{
			get;
			set;
		}

		public IDictionary<string, double> measurements
		{
			get;
			set;
		}

		public RequestData()
			: this("AI.RequestData", "RequestData")
		{
		}

		protected RequestData(string fullName, string name)
		{
			ver = 2;
			id = string.Empty;
			this.name = string.Empty;
			startTime = string.Empty;
			duration = string.Empty;
			responseCode = string.Empty;
			httpMethod = string.Empty;
			url = string.Empty;
			properties = new Dictionary<string, string>();
			measurements = new Dictionary<string, double>();
		}
	}
}
