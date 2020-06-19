using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	internal class VisualBasicCompilerBridge : CompilerBridge
	{
		internal override string DefaultFileExtension => "vb";

		protected override CommandLineParser CommandLineParser => (CommandLineParser)(object)VisualBasicCommandLineParser.Default;

		protected override SyntaxTree ParseSyntaxTree(string source, ParseOptions options)
		{
			return SyntaxFactory.ParseSyntaxTree(source, options, "", (Encoding)null, (ImmutableDictionary<string, ReportDiagnostic>)null, default(CancellationToken));
		}

		protected override Compilation CreateNewEmptyCompilation()
		{
			return (Compilation)(object)VisualBasicCompilation.Create("TemporaryT4Assembly", (IEnumerable<SyntaxTree>)null, (IEnumerable<MetadataReference>)null, (VisualBasicCompilationOptions)null);
		}

		internal VisualBasicCompilerBridge(string source, bool debug, IEnumerable<string> references, string cmdLineArguments)
			: base(source, debug, references, cmdLineArguments)
		{
		}
	}
}
