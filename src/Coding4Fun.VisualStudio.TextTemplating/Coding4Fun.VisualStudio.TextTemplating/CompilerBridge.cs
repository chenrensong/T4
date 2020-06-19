using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	internal abstract class CompilerBridge
	{
		private static IEnumerable<MetadataReference> CommonAssemblyReferences = (IEnumerable<MetadataReference>)new Assembly[1]
		{
			typeof(object).Assembly
		}.Select((Assembly x) => MetadataReference.CreateFromFile(x.Location, default(MetadataReferenceProperties), (DocumentationProvider)null));

		private readonly string sdkDirectory = RuntimeEnvironment.GetRuntimeDirectory();

		private readonly string baseDirectory = Path.GetTempPath();

		private IEnumerable<MetadataReference> templateReferences;

		private string source;

		private bool debug;

		private string rawCmdLineArguments;

		internal Guid LastCompilationId
		{
			get;
			private set;
		}

		internal abstract string DefaultFileExtension
		{
			get;
		}

		protected abstract CommandLineParser CommandLineParser
		{
			get;
		}

		protected CompilerBridge(string source, bool debug, IEnumerable<string> references, string cmdLineArguments)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (references == null)
			{
				throw new ArgumentNullException("references");
			}
			this.debug = debug;
			this.source = source;
			rawCmdLineArguments = (cmdLineArguments ?? string.Empty);
			templateReferences = references.Select((string x) => (MetadataReference)(object)MetadataReference.CreateFromFile(x, default(MetadataReferenceProperties), (DocumentationProvider)null));
		}

		/// <summary>
		/// Create a new <see cref="T:Coding4Fun.VisualStudio.TextTemplating.CompilerBridge" /> for the given <see cref="T:Coding4Fun.VisualStudio.TextTemplating.SupportedLanguage" />.
		/// </summary>
		/// <param name="language">Language of the <paramref name="source" /></param>
		/// <param name="source">To be compiled</param>
		/// <param name="debug">Include debugging symbols and output</param>
		/// <param name="references">Any specified assembly references (must be resolved to full paths).</param>
		/// <param name="cmdLineArguments">Any extra command line arguments to pass to the compilers</param>
		/// <returns></returns>
		public static CompilerBridge Create(SupportedLanguage language, string source, bool debug, IEnumerable<string> references, string cmdLineArguments)
		{
			switch (language)
			{
			case SupportedLanguage.CSharp:
				return new CSharpCompilerBridge(source, debug, references, cmdLineArguments);
			case SupportedLanguage.VB:
				return new VisualBasicCompilerBridge(source, debug, references, cmdLineArguments);
			default:
				throw new ArgumentOutOfRangeException("language");
			}
		}

		/// <summary>
		/// Create a new language specific <see cref="T:Microsoft.CodeAnalysis.Compilation" />.
		/// </summary>
		/// <remarks>
		/// Should <em>not</em> concern itself with references, syntax tree or compilation options
		/// as these will be set by the caller.
		/// </remarks>
		protected abstract Compilation CreateNewEmptyCompilation();

		/// <summary>
		/// Create a language specific <see cref="T:Microsoft.CodeAnalysis.SyntaxTree" /> from the given source.
		/// </summary>
		protected abstract SyntaxTree ParseSyntaxTree(string source, ParseOptions options);

		internal Compilation PrepareNewCompilation()
		{
			LastCompilationId = Guid.NewGuid();
			IEnumerable<string> enumerable = rawCmdLineArguments.Split(new string[1]
			{
				" "
			}, StringSplitOptions.RemoveEmptyEntries);
			CommandLineArguments val = CommandLineParser.Parse(enumerable, baseDirectory, sdkDirectory, null);
			SyntaxTree val2 = ParseSyntaxTree(source, val.ParseOptions);
			CompilationOptions val3 = val.CompilationOptions.WithOutputKind((OutputKind)2).WithOptimizationLevel((OptimizationLevel)((!debug) ? 1 : 0))
                .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
			IEnumerable<MetadataReference> second = from x in ImmutableArrayExtensions.Select(val.MetadataReferences, (CommandLineReference x) => x.Reference)
                                                    select MetadataReference.CreateFromFile(x, default, null);
			IEnumerable<MetadataReference> enumerable2 = CommonAssemblyReferences.Union(templateReferences).Union(second);
			return CreateNewEmptyCompilation().AddSyntaxTrees((SyntaxTree[])(object)new SyntaxTree[1]
			{
				val2
			}).WithOptions(val3).WithReferences(enumerable2);
		}

		/// <summary>
		/// Compile a new Compilation using this <see cref="T:Coding4Fun.VisualStudio.TextTemplating.CompilerBridge" />.
		/// </summary>
		public CompilerBridgeOutput Compile()
		{
			Compilation val = PrepareNewCompilation();
			Assembly compiledAssembly = null;
			EmitResult val2;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (MemoryStream memoryStream2 = new MemoryStream())
				{
					val2 = val.Emit(memoryStream, memoryStream2, null, null, null, null, null, null, null, null, default(CancellationToken));
					if (debug)
					{
						EmitDebugInformation(source, (IEnumerable<Diagnostic>)(object)val2.Diagnostics);
					}
					if (val2.Success)
					{
						if (debug)
						{
							EmitBinaries(memoryStream, memoryStream2);
							compiledAssembly = Assembly.Load(memoryStream.ToArray(), memoryStream2.ToArray());
						}
						else
						{
							compiledAssembly = Assembly.Load(memoryStream.ToArray());
						}
					}
				}
			}
			return new CompilerBridgeOutput(val2, compiledAssembly);
		}

		internal string TemporaryFilePathForLastCompilation(string extension)
		{
			string path = baseDirectory;
			string path2 = LastCompilationId.ToString("N");
			path2 = Path.ChangeExtension(path2, extension);
			return Path.Combine(path, path2);
		}

		private void EmitDebugInformation(string source, IEnumerable<Diagnostic> diagnostics)
		{
			string path = TemporaryFilePathForLastCompilation(DefaultFileExtension);
			File.WriteAllText(path, source);
			IEnumerable<string> contents = diagnostics.Select((Diagnostic x) => ((object)x).ToString());
			string path2 = TemporaryFilePathForLastCompilation("log");
			File.WriteAllLines(path2, contents);
		}

		private void EmitBinaries(Stream peStream, Stream pdbStream)
		{
			string path = TemporaryFilePathForLastCompilation("dll");
			string path2 = TemporaryFilePathForLastCompilation("pdb");
			using (FileStream destination = new FileStream(path, FileMode.Create))
			{
				using (FileStream destination2 = new FileStream(path2, FileMode.Create))
				{
					peStream.Position = 0L;
					peStream.CopyTo(destination);
					pdbStream.Position = 0L;
					pdbStream.CopyTo(destination2);
				}
			}
		}
	}
}
