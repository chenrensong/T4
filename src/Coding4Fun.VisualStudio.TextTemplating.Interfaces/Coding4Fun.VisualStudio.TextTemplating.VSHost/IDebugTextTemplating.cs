using System;

namespace Coding4Fun.VisualStudio.TextTemplating.VSHost
{
	[CLSCompliant(false)]
	public interface IDebugTextTemplating : ITextTemplating
	{
		event EventHandler<DebugTemplateEventArgs> DebugCompleted;

		void DebugTemplateAsync(string inputFilename, string content, ITextTemplatingCallback callback, object hierarchy);
	}
}
