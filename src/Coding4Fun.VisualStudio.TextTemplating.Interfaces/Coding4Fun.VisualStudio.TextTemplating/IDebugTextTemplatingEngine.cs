using System;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	[CLSCompliant(true)]
	public interface IDebugTextTemplatingEngine : ITextTemplatingEngine
	{
		/// <returns>Returns <see cref="T:Coding4Fun.VisualStudio.TextTemplating.IDebugTransformationRun" />.</returns>
		IDebugTransformationRun PrepareTransformationRun(string content, ITextTemplatingEngineHost host, IDebugTransformationRunFactory runFactory);
	}
}
