using System;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	[CLSCompliant(true)]
	public interface ITextTemplatingEngine
	{
		string ProcessTemplate(string content, ITextTemplatingEngineHost host);

		string PreprocessTemplate(string content, ITextTemplatingEngineHost host, string className, string classNamespace, out string language, out string[] references);
	}
}
