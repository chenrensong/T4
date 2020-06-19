using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_ExceptionData")]
	internal class ExceptionData
	{
		public int ver
		{
			get;
			set;
		}

		public string handledAt
		{
			get;
			set;
		}

		public IList<ExceptionDetails> exceptions
		{
			get;
			set;
		}

		public SeverityLevel? severityLevel
		{
			get;
			set;
		}

		public string problemId
		{
			get;
			set;
		}

		public int crashThreadId
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

		public ExceptionData()
			: this("AI.ExceptionData", "ExceptionData")
		{
		}

		protected ExceptionData(string fullName, string name)
		{
			ver = 2;
			handledAt = string.Empty;
			exceptions = new List<ExceptionDetails>();
			problemId = string.Empty;
			properties = new Dictionary<string, string>();
			measurements = new Dictionary<string, double>();
		}
	}
}
