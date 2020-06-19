using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	[CLSCompliant(true)]
	public interface ITextTemplatingEngineHost
	{
		IList<string> StandardAssemblyReferences
		{
			get;
		}

		IList<string> StandardImports
		{
			get;
		}

		string TemplateFile
		{
			get;
		}

		bool LoadIncludeText(string requestFileName, out string content, out string location);

		string ResolveAssemblyReference(string assemblyReference);

		Type ResolveDirectiveProcessor(string processorName);

		string ResolvePath(string path);

		string ResolveParameterValue(string directiveId, string processorName, string parameterName);

        AssemblyLoadContext ProvideTemplatingAppDomain(string content);

		void LogErrors(CompilerErrorCollection errors);

		void SetFileExtension(string extension);

		void SetOutputEncoding(Encoding encoding, bool fromOutputDirective);

		object GetHostOption(string optionName);
	}
}
