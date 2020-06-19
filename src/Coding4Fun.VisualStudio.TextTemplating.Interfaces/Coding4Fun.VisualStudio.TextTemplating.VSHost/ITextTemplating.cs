using System;

namespace Coding4Fun.VisualStudio.TextTemplating.VSHost
{
	[CLSCompliant(false)]
	public interface ITextTemplating
	{
		string ProcessTemplate(string inputFile, string content, ITextTemplatingCallback callback = null, object hierarchy = null);

		string PreprocessTemplate(string inputFile, string content, ITextTemplatingCallback callback, string className, string classNamespace, out string[] references);

		void BeginErrorSession();

		bool EndErrorSession();
	}
}
