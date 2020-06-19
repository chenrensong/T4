using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData]
	internal class DataPoint
	{
		public string name
		{
			get;
			set;
		}

		public DataPointType kind
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

		public DataPoint()
			: this("AI.DataPoint", "DataPoint")
		{
		}

		protected DataPoint(string fullName, string name)
		{
			this.name = string.Empty;
			kind = DataPointType.Measurement;
		}
	}
}
