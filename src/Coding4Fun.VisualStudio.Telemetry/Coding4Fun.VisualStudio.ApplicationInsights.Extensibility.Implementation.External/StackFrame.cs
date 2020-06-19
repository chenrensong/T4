using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData]
	internal class StackFrame
	{
		public int level
		{
			get;
			set;
		}

		public string method
		{
			get;
			set;
		}

		public string assembly
		{
			get;
			set;
		}

		public string fileName
		{
			get;
			set;
		}

		public int line
		{
			get;
			set;
		}

		public StackFrame()
			: this("AI.StackFrame", "StackFrame")
		{
		}

		protected StackFrame(string fullName, string name)
		{
			method = string.Empty;
			assembly = string.Empty;
			fileName = string.Empty;
		}
	}
}
