using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_EventData")]
	internal class EventData
	{
		public int ver
		{
			get;
			set;
		}

		public string name
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

		public EventData()
			: this("AI.EventData", "EventData")
		{
		}

		protected EventData(string fullName, string name)
		{
			ver = 2;
			this.name = string.Empty;
			properties = new Dictionary<string, string>();
			measurements = new Dictionary<string, double>();
		}
	}
}
