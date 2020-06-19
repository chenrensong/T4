using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	internal class CSharpCompilerBridge : CompilerBridge
	{
		internal override string DefaultFileExtension => "cs";

		protected override CommandLineParser CommandLineParser => (CommandLineParser)(object)CSharpCommandLineParser.Default;

		protected override SyntaxTree ParseSyntaxTree(string source, ParseOptions options)
		{
			return SyntaxFactory.ParseSyntaxTree(source, options, "", (Encoding)null, (ImmutableDictionary<string, ReportDiagnostic>)null, default(CancellationToken));
		}

		protected override Compilation CreateNewEmptyCompilation()
		{
			return (Compilation)(object)CSharpCompilation.Create("TemporaryT4Assembly", (IEnumerable<SyntaxTree>)null, (IEnumerable<MetadataReference>)null, (CSharpCompilationOptions)null);
		}

		internal CSharpCompilerBridge(string source, bool debug, IEnumerable<string> references, string cmdLineArguments)
			: base(source, debug, references, cmdLineArguments)
		{
		}
	}
}
