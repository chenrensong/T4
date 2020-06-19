using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_MessageData")]
	internal class MessageData
	{
		public int ver
		{
			get;
			set;
		}

		public string message
		{
			get;
			set;
		}

		public SeverityLevel? severityLevel
		{
			get;
			set;
		}

		public IDictionary<string, string> properties
		{
			get;
			set;
		}

		public MessageData()
			: this("AI.MessageData", "MessageData")
		{
		}

		protected MessageData(string fullName, string name)
		{
			ver = 2;
			message = string.Empty;
			properties = new Dictionary<string, string>();
		}
	}
}
