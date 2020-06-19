using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_PerformanceCounterData")]
	internal class PerformanceCounterData
	{
		public int ver
		{
			get;
			set;
		}

		public string categoryName
		{
			get;
			set;
		}

		public string counterName
		{
			get;
			set;
		}

		public string instanceName
		{
			get;
			set;
		}

		public DataPointType kind
		{
			get;
			set;
		}

		public int? count
		{
			get;
			set;
		}

		public double? min
		{
			get;
			set;
		}

		public double? max
		{
			get;
			set;
		}

		public double? stdDev
		{
			get;
			set;
		}

		public double value
		{
			[CompilerGenerated]
			get
			{
				return value;
			}
			[CompilerGenerated]
			set
			{
				this.value = value;
			}
		}

		public IDictionary<string, string> properties
		{
			get;
			set;
		}

		public PerformanceCounterData()
			: this("AI.PerformanceCounterData", "PerformanceCounterData")
		{
		}

		protected PerformanceCounterData(string fullName, string name)
		{
			ver = 2;
			categoryName = string.Empty;
			counterName = string.Empty;
			instanceName = string.Empty;
			kind = DataPointType.Aggregation;
			properties = new Dictionary<string, string>();
		}
	}
}
