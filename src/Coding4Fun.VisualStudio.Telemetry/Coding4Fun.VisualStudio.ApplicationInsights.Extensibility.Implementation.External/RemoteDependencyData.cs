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
	[EventData(Name = "PartB_RemoteDependencyData")]
	internal class RemoteDependencyData
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

		public DependencyKind dependencyKind
		{
			get;
			set;
		}

		public bool? success
		{
			get;
			set;
		}

		public bool? async
		{
			get;
			set;
		}

		public DependencySourceType dependencySource
		{
			get;
			set;
		}

		public string commandName
		{
			get;
			set;
		}

		public string dependencyTypeName
		{
			get;
			set;
		}

		public IDictionary<string, string> properties
		{
			get;
			set;
		}

		public RemoteDependencyData()
			: this("AI.RemoteDependencyData", "RemoteDependencyData")
		{
		}

		protected RemoteDependencyData(string fullName, string name)
		{
			ver = 2;
			this.name = string.Empty;
			kind = DataPointType.Measurement;
			dependencyKind = DependencyKind.Other;
			success = true;
			dependencySource = DependencySourceType.Undefined;
			commandName = string.Empty;
			dependencyTypeName = string.Empty;
			properties = new Dictionary<string, string>();
		}
	}
}
