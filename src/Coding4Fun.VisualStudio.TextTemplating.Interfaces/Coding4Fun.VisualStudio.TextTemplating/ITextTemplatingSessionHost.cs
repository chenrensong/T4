using System;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	[CLSCompliant(true)]
	public interface ITextTemplatingSessionHost
	{
		ITextTemplatingSession Session
		{
			get;
			set;
		}

		ITextTemplatingSession CreateSession();
	}
}
