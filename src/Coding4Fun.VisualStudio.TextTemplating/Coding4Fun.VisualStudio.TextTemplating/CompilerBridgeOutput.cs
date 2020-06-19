using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	internal class CompilerBridgeOutput
	{
		private readonly EmitResult rawEmitResult;

		public Assembly CompiledAssembly
		{
			get;
			private set;
		}

		public bool Successful => rawEmitResult.Success;
		public IEnumerable<CompilerError> Diagnostics => CreateCompilerErrorFromDiagnostic((IEnumerable<Diagnostic>)(object)rawEmitResult.Diagnostics);

		public CompilerBridgeOutput(EmitResult emitResult, Assembly compiledAssembly)
		{
			rawEmitResult = emitResult;
			CompiledAssembly = compiledAssembly;
		}

		internal static IEnumerable<CompilerError> CreateCompilerErrorFromDiagnostic(IEnumerable<Diagnostic> diagnostics)
		{
			foreach (Diagnostic item in diagnostics.Where((Diagnostic x) => (int)x.Severity != 0 && (int)x.Severity != 1))
			{
				FileLinePositionSpan mappedLineSpan = item.Location.GetMappedLineSpan();
				LinePosition startLinePosition = ((FileLinePositionSpan)(mappedLineSpan)).StartLinePosition;
				int line = ((LinePosition)(startLinePosition)).Line + 1;
				startLinePosition = ((FileLinePositionSpan)(mappedLineSpan)).StartLinePosition;
				int column = ((LinePosition)(startLinePosition)).Character + 1;
				yield return new CompilerError(((FileLinePositionSpan)(mappedLineSpan)).Path, line, column, item.Id, item.GetMessage((IFormatProvider)null))
				{
					IsWarning = ((int)item.Severity == 2)
				};
			}
		}
	}
}
