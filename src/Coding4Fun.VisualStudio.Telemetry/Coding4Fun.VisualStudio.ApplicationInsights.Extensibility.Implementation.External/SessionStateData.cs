using System.Diagnostics.Tracing;
using System.CodeDom.Compiler;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData(Name = "PartB_SessionStateData")]
	internal class SessionStateData
	{
		public int ver
		{
			get;
			set;
		}

		public SessionState state
		{
			get;
			set;
		}

		public SessionStateData()
			: this("AI.SessionStateData", "SessionStateData")
		{
		}

		protected SessionStateData(string fullName, string name)
		{
			ver = 2;
			state = SessionState.Start;
		}
	}
}
