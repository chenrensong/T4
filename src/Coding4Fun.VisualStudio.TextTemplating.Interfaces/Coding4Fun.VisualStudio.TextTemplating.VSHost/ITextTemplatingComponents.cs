using System;

namespace Coding4Fun.VisualStudio.TextTemplating.VSHost
{
	[CLSCompliant(false)]
	public interface ITextTemplatingComponents
	{
		ITextTemplatingEngineHost Host
		{
			get;
		}

		ITextTemplatingEngine Engine
		{
			get;
		}

		string InputFile
		{
			get;
			set;
		}

		ITextTemplatingCallback Callback
		{
			get;
			set;
		}

		object Hierarchy
		{
			get;
			set;
		}
	}
}
