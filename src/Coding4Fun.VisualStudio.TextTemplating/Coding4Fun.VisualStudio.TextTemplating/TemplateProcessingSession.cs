using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Class to carry the data associated with generation from one templated file 
	/// </summary>
	/// <remarks>
	/// Note - this is not the same as the ITextTransformationSession which represents an end user's batched session of multiple transforms.
	/// The lifetime of this class is one call to ProcessTemplate or PreprocessTemplate.
	/// </remarks>
	[Serializable]
	internal sealed class TemplateProcessingSession : IDisposable
	{
		private bool preprocess;

		private readonly List<string> assemblyDirectives = new List<string>();

		private readonly List<string> importDirectives = new List<string>();

		private string className;

		private string baseClassName;

		[NonSerialized]
		private CodeDomProvider codeDomProvider;

		private bool debug;

		private HostSpecific hostSpecific;

		private bool processedOutputDirective;

		private bool processedTemplateDirective;

		private CultureInfo formatProvider;

		private SupportedLanguage language;

		private readonly Dictionary<string, string> languageOptions = new Dictionary<string, string>();

		private readonly Stack<string> includeStack = new Stack<string>();

		private string templateContents;

		private string templateFile;

		private bool cacheAssemblies;

		private string compilerOptions;

		private ITextTemplatingSession userTransformationSession;

		private bool isPublic = true;

		private bool linePragmas = true;

		/// <summary>
		/// Whether this session is for precompilation or regular transformation
		/// </summary>
		public bool Preprocess
		{
			[DebuggerStepThrough]
			get
			{
				return preprocess;
			}
			[DebuggerStepThrough]
			set
			{
				preprocess = value;
			}
		}

		/// <summary>
		/// List of full names of assemblies that need to be referenced
		/// when compiling/running the transformation code
		/// </summary>
		public List<string> AssemblyDirectives
		{
			[DebuggerStepThrough]
			get
			{
				return assemblyDirectives;
			}
		}

		/// <summary>
		/// List of namespaces to be imported in the transformation code
		/// </summary>
		public List<string> ImportDirectives
		{
			[DebuggerStepThrough]
			get
			{
				return importDirectives;
			}
		}

		/// <summary>
		/// The full name of the class that will be created to do the generation
		/// </summary>
		/// <value></value>
		public string ClassFullName
		{
			[DebuggerStepThrough]
			get
			{
				return className;
			}
			[DebuggerStepThrough]
			set
			{
				className = value;
			}
		}

		/// <summary>
		/// The base class for the generated transformation class
		/// </summary>
		public string BaseClassName
		{
			[DebuggerStepThrough]
			get
			{
				return baseClassName;
			}
			[DebuggerStepThrough]
			set
			{
				baseClassName = value;
			}
		}

		/// <summary>
		/// The language that the template is written in. Defaults
		/// to CSharp
		/// </summary>
		public SupportedLanguage Language
		{
			[DebuggerStepThrough]
			get
			{
				return language;
			}
			[DebuggerStepThrough]
			set
			{
				language = value;
			}
		}

		/// <summary>
		/// Options set for the language of the template.
		/// </summary>
		/// <remarks>
		/// These options are passed directly to the CodeDOM provider
		/// </remarks>
		public IDictionary<string, string> LanguageOptions
		{
			[DebuggerStepThrough]
			get
			{
				return languageOptions;
			}
		}

		/// <summary>
		/// The CodeDomProvider used to build up the CodeDom tree of the code
		/// for the transformation class. This is constructed based on the 
		/// Language property.
		/// </summary>
		public CodeDomProvider CodeDomProvider
		{
			[DebuggerStepThrough]
			get
			{
				if (codeDomProvider == null)
				{
					if (Language == SupportedLanguage.VB)
					{
						codeDomProvider = new VBCodeProvider(LanguageOptions);
					}
					else
					{
						codeDomProvider = new CSharpCodeProvider(LanguageOptions);
					}
				}
				return codeDomProvider;
			}
		}

		/// <summary>
		/// Whether the transformation code is compiled in debug-mode.
		/// </summary>
		public bool Debug
		{
			[DebuggerStepThrough]
			get
			{
				return debug;
			}
			[DebuggerStepThrough]
			set
			{
				debug = value;
			}
		}

		/// <summary>
		/// Whether the template is host-specific or not.
		/// </summary>
		/// <remarks>
		/// If it is host-specific, 
		/// then a 'Host' property will be generated in the transformation code.
		/// If it is TrueFromBase then the 'Host' property will be assumed to be defined in the base class.
		/// </remarks>
		public HostSpecific HostSpecific
		{
			[DebuggerStepThrough]
			get
			{
				return hostSpecific;
			}
			[DebuggerStepThrough]
			set
			{
				hostSpecific = value;
			}
		}

		/// <summary>
		/// Is the templete host speciifc either directly or from its base class.
		/// </summary>
		public bool IsHostSpecific
		{
			get
			{
				if (hostSpecific != HostSpecific.True)
				{
					return hostSpecific == HostSpecific.TrueFromBase;
				}
				return true;
			}
		}

		/// <summary>
		/// Whether or not we have already processed an output directive for this template file
		/// </summary>
		public bool ProcessedOutputDirective
		{
			[DebuggerStepThrough]
			get
			{
				return processedOutputDirective;
			}
			[DebuggerStepThrough]
			set
			{
				processedOutputDirective = value;
			}
		}

		/// <summary>
		/// Whether or not we have already processed a template directive for this template file
		/// </summary>
		public bool ProcessedTemplateDirective
		{
			[DebuggerStepThrough]
			get
			{
				return processedTemplateDirective;
			}
			[DebuggerStepThrough]
			set
			{
				processedTemplateDirective = value;
			}
		}

		/// <summary>
		/// The FormatProvider to be used to convert expressions to strings. This is
		/// a CultureInfo object constructed from the culture specified in the template
		/// directive
		/// </summary>
		public CultureInfo FormatProvider
		{
			[DebuggerStepThrough]
			get
			{
				if (formatProvider == null)
				{
					formatProvider = CultureInfo.InvariantCulture;
				}
				return formatProvider;
			}
			[DebuggerStepThrough]
			set
			{
				if (value != null)
				{
					formatProvider = value;
				}
			}
		}

		/// <summary>
		/// The stack of files included
		/// </summary>
		public Stack<string> IncludeStack
		{
			[DebuggerStepThrough]
			get
			{
				return includeStack;
			}
		}

		/// <summary>
		/// The contents of the template being processed
		/// </summary>
		public string TemplateContents
		{
			[DebuggerStepThrough]
			get
			{
				return templateContents;
			}
			[DebuggerStepThrough]
			set
			{
				templateContents = value;
			}
		}

		/// <summary>
		/// The path of the outermost template being processed
		/// </summary>
		/// <remarks>
		/// May be null for non file-based hosts.
		/// </remarks>
		public string TemplateFile
		{
			[DebuggerStepThrough]
			get
			{
				return templateFile;
			}
			[DebuggerStepThrough]
			set
			{
				templateFile = value;
			}
		}

		/// <summary>
		/// Whether to cache assemblies for this session.
		/// </summary>
		public bool CacheAssemblies
		{
			get
			{
				return cacheAssemblies;
			}
			set
			{
				cacheAssemblies = value;
			}
		}

		/// <summary>
		/// Options to send to the compiler for the template control block code
		/// </summary>
		public string CompilerOptions
		{
			get
			{
				return compilerOptions;
			}
			set
			{
				compilerOptions = value;
			}
		}

		/// <summary>
		/// Client-provided session object to flow through to the runtime AppDomain.
		/// </summary>
		public ITextTemplatingSession UserTransformationSession
		{
			get
			{
				return userTransformationSession;
			}
			set
			{
				userTransformationSession = value;
			}
		}

		/// <summary>
		/// Whether the generated class is public or internal.
		/// </summary>
		public bool IsPublic
		{
			get
			{
				return isPublic;
			}
			set
			{
				isPublic = value;
			}
		}

		/// <summary>
		/// Whether the generated class contains line pragmas to redirect errors and the debugger to the template.
		/// </summary>
		public bool LinePragmas
		{
			get
			{
				return linePragmas;
			}
			set
			{
				linePragmas = value;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~TemplateProcessingSession()
		{
			Dispose(false);
		}

		private void Dispose(bool dispose)
		{
			if (dispose && codeDomProvider != null)
			{
				codeDomProvider.Dispose();
				codeDomProvider = null;
			}
		}
	}
}
