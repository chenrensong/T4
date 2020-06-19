using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_MetricData")]
	internal class MetricData
	{
		public int ver
		{
			get;
			set;
		}

		public IList<DataPoint> metrics
		{
			get;
			set;
		}

		public IDictionary<string, string> properties
		{
			get;
			set;
		}

		public MetricData()
			: this("AI.MetricData", "MetricData")
		{
		}

		protected MetricData(string fullName, string name)
		{
			ver = 2;
			metrics = new List<DataPoint>();
			properties = new Dictionary<string, string>();
		}
	}
}
