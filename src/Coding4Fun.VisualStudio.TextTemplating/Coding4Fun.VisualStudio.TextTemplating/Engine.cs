using Coding4Fun.VisualStudio.ArchitectureTools.Telemetry;
using Coding4Fun.VisualStudio.TextTemplating.CodeDom;
using Coding4Fun.VisualStudio.TextTemplating.Properties;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Text templating engine
	/// </summary>
	public class Engine : ITextTemplatingEngine, IDebugTextTemplatingEngine, IDisposable
	{
		/// <summary>
		/// Special visited set to encapsulate the recording of file inclusions for the "once" feature of the include directive
		/// </summary>
		private class VisitedFiles
		{
			private HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			/// <summary>
			/// Record a "visit" to an include file. Return whether the file has already been visited
			/// </summary>
			/// <param name="fileLocation">Absolute path to the include file in canonical form</param>
			/// <returns>true if previously visited, or false if first visit</returns>
			public bool Visit(string fileLocation)
			{
				if (visited.Contains(fileLocation))
				{
					return true;
				}
				visited.Add(fileLocation);
				return false;
			}
		}

		/// <summary>
		/// Hashing algorithm to create cache keys for fingerprints of templates.
		/// </summary>		
		private static readonly SHA512CryptoServiceProvider hasher = new SHA512CryptoServiceProvider();

		private ITelemetryService telemetryService;

		private bool isDisposed;

		/// <summary>
		/// CacheAssemblies option string 
		/// </summary>
		public const string CacheAssembliesOptionString = "CacheAssemblies";

		public const string TemplateFileParameterName = "TemplateFile";

		public const string TransformAllHostOption = "isTransformAll";

		private const string IncludeDirectiveName = "include";

		private const string AssemblyDirectiveName = "assembly";

		private const string ImportDirectiveName = "import";

		private const string TemplateDirectiveName = "template";

		private const string OutputDirectiveName = "output";

		private const string MethodName = "TransformText";

		private readonly CompilerErrorCollection errors = new CompilerErrorCollection();

		public Engine()
			: this(new VSTelemetryService())
		{
		}

		internal Engine(ITelemetryService telemetryService)
		{
			this.telemetryService = telemetryService;
		}

		~Engine()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				telemetryService?.Dispose();
				isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Processes a template
		/// </summary>
		/// <param name="content">The contents of the template file to be processed</param>
		/// <param name="host">The ITextTemplatingEngineHost that will host this engine</param>
		/// <returns>The output from the processed template</returns>
		public string ProcessTemplate(string content, ITextTemplatingEngineHost host)
		{
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			TemplateProcessingSession session = new TemplateProcessingSession
			{
				TemplateContents = content
			};
			InitializeSessionWithHostData(host, session);
			errors.Clear();
			using (ITelemetryScope scope = telemetryService.StartUserTask(T4TelemetryEvent.ProcessTemplate))
			{
				return ProcessTemplateImplementation(session, content, host, null, null, scope);
			}
		}

		/// <summary>
		/// Process the contents of a templated file running inline code to produce a class that represents the template.
		/// </summary>
		/// <param name="content">The content of the templated file</param>
		/// <param name="host">The hosting environment using this engine</param>
		/// <param name="className">The name of the class to produce</param>
		/// <param name="classNamespace">The namespace of the class to produce</param>
		/// <param name="language">The language that the template's control code was written in</param>
		/// <param name="references">The set of references required by the template</param>
		/// <returns></returns>
		public string PreprocessTemplate(string content, ITextTemplatingEngineHost host, string className, string classNamespace, out string language, out string[] references)
		{
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (string.IsNullOrEmpty(className))
			{
				throw new ArgumentNullException("className");
			}
			if (classNamespace == null)
			{
				classNamespace = string.Empty;
			}
			TemplateProcessingSession templateProcessingSession = new TemplateProcessingSession
			{
				TemplateContents = content,
				Preprocess = true
			};
			InitializeSessionWithHostData(host, templateProcessingSession);
			errors.Clear();
			using (ITelemetryScope scope = telemetryService.StartUserTask(T4TelemetryEvent.PreprocessTemplate))
			{
				try
				{
					return ProcessTemplateImplementation(templateProcessingSession, content, host, className, classNamespace, scope);
				}
				finally
				{
					language = templateProcessingSession.Language.ToString();
					references = templateProcessingSession.AssemblyDirectives.ToArray();
				}
			}
		}

		/// <summary>
		/// Do the main processing of the template that is shared between preprocessing and regular processing
		/// </summary>
		private string ProcessTemplateImplementation(TemplateProcessingSession session, string content, ITextTemplatingEngineHost host, string className, string classNamespace, ITelemetryScope scope)
		{
			string result = Resources.ErrorOutput;
			scope.EndEvent.SetProperty(T4TelemetryProperty.TransformAllProperty, HostIsSetToTransformAll(host));
			try
			{
				CallContext.SetData("TemplateFile", session.TemplateFile);
				string text = ParseAndConstructGeneratorCode(session, content, host, className, classNamespace, scope);
				if (!session.Preprocess)
				{
					if (!errors.HasErrors)
					{
						NonDebugRunFactory runFactory = new NonDebugRunFactory(host);
						IDebugTransformationRun debugTransformationRun = null;
						try
						{
							debugTransformationRun = CompileAndPrepareRun(text, host, session, runFactory, scope);
							if (debugTransformationRun != null)
							{
								using (scope.StartOperation(T4TelemetryEvent.TransformationOperation))
								{
									result = debugTransformationRun.PerformTransformation();
									return result;
								}
							}
							return result;
						}
						catch (SerializationException)
						{
							LogError(session.TemplateFile, -1, -1, Resources.SessionHostMarshalError, false);
							throw;
						}
						finally
						{
							if (debugTransformationRun != null)
							{
								errors.AddRange(debugTransformationRun.Errors);
							}
						}
					}
					return result;
				}
				result = text;
				return result;
			}
			catch (Exception ex2)
			{
				scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex2);
				if (IsCriticalException(ex2))
				{
					throw;
				}
				LogError(session.TemplateFile, -1, -1, Resources.ExceptionProcessingTemplate + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex2), false);
				return result;
			}
			finally
			{
				CallContext.FreeNamedDataSlot("TemplateFile");
				session.IncludeStack.Clear();
				host.LogErrors(errors);
			}
		}

		private static bool HostIsSetToTransformAll(ITextTemplatingEngineHost host)
		{
			try
			{
				object hostOption = host.GetHostOption("isTransformAll");
				if (hostOption is bool)
				{
					return (bool)hostOption;
				}
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// Set up the given session with data gleaned from callback to the host.
		/// </summary>
		private static void InitializeSessionWithHostData(ITextTemplatingEngineHost host, TemplateProcessingSession session)
		{
			try
			{
				session.TemplateFile = host.TemplateFile;
			}
			catch (NotImplementedException)
			{
				session.TemplateFile = string.Empty;
			}
			session.IncludeStack.Push(session.TemplateFile);
			ITextTemplatingSessionHost textTemplatingSessionHost = host as ITextTemplatingSessionHost;
			if (textTemplatingSessionHost != null)
			{
				session.UserTransformationSession = textTemplatingSessionHost.Session;
			}
		}

		/// <summary>
		/// Prepare an IDebugTransformationRun object to run the template.
		/// </summary>
		/// <param name="content">The content of the templated file</param>
		/// <param name="host">The hosting environment using this engine</param>
		/// <param name="runFactory">The hosting environment for the transformation run</param>
		/// <returns></returns>
		public IDebugTransformationRun PrepareTransformationRun(string content, ITextTemplatingEngineHost host, IDebugTransformationRunFactory runFactory)
		{
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			if (runFactory == null)
			{
				throw new ArgumentNullException("runFactory");
			}
			TemplateProcessingSession templateProcessingSession = new TemplateProcessingSession
			{
				TemplateContents = content,
				Debug = true
			};
			using (ITelemetryScope telemetryScope = telemetryService.StartUserTask(T4TelemetryEvent.DebugTemplate))
			{
				InitializeSessionWithHostData(host, templateProcessingSession);
				errors.Clear();
				IDebugTransformationRun result = null;
				try
				{
					string generatorCode = ParseAndConstructGeneratorCode(templateProcessingSession, content, host, null, null, telemetryScope);
					if (errors.HasErrors)
					{
						return null;
					}
					templateProcessingSession.Debug = true;
					result = CompileAndPrepareRun(generatorCode, host, templateProcessingSession, runFactory, telemetryScope);
				}
				catch (Exception ex)
				{
					telemetryScope.PostFault(T4TelemetryEvent.TransformationFault, null, ex);
					if (IsCriticalException(ex))
					{
						throw;
					}
					LogError(templateProcessingSession.TemplateFile, -1, -1, Resources.ExceptionProcessingTemplate + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
				}
				finally
				{
					templateProcessingSession.IncludeStack.Clear();
					host.LogErrors(errors);
				}
				return result;
			}
		}

		/// <summary>
		/// Parse the template code and construct the generator code from it
		/// </summary>
		private string ParseAndConstructGeneratorCode(TemplateProcessingSession session, string content, ITextTemplatingEngineHost host, string className, string classNamespace, ITelemetryScope scope)
		{
			string errorOutput = Resources.ErrorOutput;
			using (ITelemetryScope scope2 = scope.StartOperation(T4TelemetryEvent.ParseOperation))
			{
				List<Block> blocks = TemplateParser.ParseTemplateIntoBlocks(content, session.TemplateFile, errors);
				if (errors.HasErrors)
				{
					return errorOutput;
				}
				ProcessDirectives(blocks, host, session, scope2, out CodeAttributeDeclarationCollection templateClassCustomAttributes);
				if (errors.HasErrors)
				{
					return errorOutput;
				}
				return ConstructGeneratorCode(host, blocks, session, true, className, classNamespace, templateClassCustomAttributes, scope2);
			}
		}

		/// <summary>
		/// Utility function for logging an error
		/// </summary>
		/// <param name="block"></param>
		/// <param name="errorText"></param>
		/// <param name="isWarning"></param>
		private void LogError(Block block, string errorText, bool isWarning)
		{
			string fileName = "";
			if (!string.IsNullOrEmpty(block.FileName))
			{
				fileName = block.FileName;
			}
			LogError(fileName, block.StartLineNumber, block.StartColumnNumber, errorText, isWarning);
		}

		/// <summary>
		/// Utility function for logging an error
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="line"></param>
		/// <param name="column"></param>
		/// <param name="errorText"></param>
		/// <param name="isWarning"></param>
		private void LogError(string fileName, int line, int column, string errorText, bool isWarning)
		{
			CompilerError value = new CompilerError(fileName, line, column, null, errorText)
			{
				IsWarning = isWarning
			};
			errors.Add(value);
		}

		/// <summary>
		/// Processes the directives in the template
		/// </summary>
		/// <param name="blocks">The blocks that the template was parsed into</param>
		/// <param name="host">host</param>
		/// <param name="session">TemplateProcessingSession to store data for this session</param>
		/// <param name="templateClassCustomAttributes">Collection of attributes to decorate the template class with.</param>
		private void ProcessDirectives(List<Block> blocks, ITextTemplatingEngineHost host, TemplateProcessingSession session, ITelemetryScope scope, out CodeAttributeDeclarationCollection templateClassCustomAttributes)
		{
			IEnumerable<Directive> directivesToBeProcessed = ProcessBuiltInDirectives(blocks, host, session, scope);
			TemplateParser.StripExtraNewlines(blocks);
			Dictionary<string, IDirectiveProcessor> dictionary = ProcessCustomDirectives(host, session, scope, directivesToBeProcessed);
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			StringBuilder stringBuilder3 = new StringBuilder();
			CodeAttributeDeclarationCollection codeAttributeDeclarationCollection = new CodeAttributeDeclarationCollection();
			foreach (string key in dictionary.Keys)
			{
				IDirectiveProcessor directiveProcessor = dictionary[key];
				try
				{
					directiveProcessor.FinishProcessingRun();
					string preInitializationCodeForProcessingRun = directiveProcessor.GetPreInitializationCodeForProcessingRun();
					if (!string.IsNullOrEmpty(preInitializationCodeForProcessingRun))
					{
						stringBuilder2.AppendLine(preInitializationCodeForProcessingRun);
					}
					string postInitializationCodeForProcessingRun = directiveProcessor.GetPostInitializationCodeForProcessingRun();
					if (!string.IsNullOrEmpty(postInitializationCodeForProcessingRun))
					{
						stringBuilder3.AppendLine(postInitializationCodeForProcessingRun);
					}
					string classCodeForProcessingRun = directiveProcessor.GetClassCodeForProcessingRun();
					if (!string.IsNullOrEmpty(classCodeForProcessingRun))
					{
						stringBuilder.AppendLine(classCodeForProcessingRun);
					}
					CodeAttributeDeclarationCollection templateClassCustomAttributes2 = directiveProcessor.GetTemplateClassCustomAttributes();
					if (templateClassCustomAttributes2 != null && templateClassCustomAttributes2.Count > 0)
					{
						codeAttributeDeclarationCollection.AddRange(templateClassCustomAttributes2);
					}
				}
				catch (Exception ex)
				{
					if (IsCriticalException(ex))
					{
						throw;
					}
					scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex);
					LogError(session.TemplateFile, -1, -1, string.Format(CultureInfo.CurrentCulture, Resources.ExceptionGettingProcessorOutput, key) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
				}
				try
				{
					string[] importsForProcessingRun = directiveProcessor.GetImportsForProcessingRun();
					if (importsForProcessingRun != null)
					{
						session.ImportDirectives.AddRange(importsForProcessingRun);
					}
					string[] referencesForProcessingRun = directiveProcessor.GetReferencesForProcessingRun();
					if (referencesForProcessingRun != null)
					{
						session.AssemblyDirectives.AddRange(referencesForProcessingRun);
					}
				}
				catch (Exception ex2)
				{
					if (IsCriticalException(ex2))
					{
						throw;
					}
					scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex2);
					LogError(session.TemplateFile, -1, -1, string.Format(CultureInfo.CurrentCulture, Resources.ExceptionGettingReferencesFromDP, key) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex2), false);
				}
			}
			try
			{
				IList<string> standardImports = host.StandardImports;
				if (standardImports != null)
				{
					session.ImportDirectives.AddRange(standardImports);
				}
				IList<string> standardAssemblyReferences = host.StandardAssemblyReferences;
                var runtimeReferences = DependentAssemblyManager.GetRuntimeAssemblies();
                if (runtimeReferences.Count() > 0)
                {
                    session.AssemblyDirectives.AddRange(runtimeReferences);
                }
                if (standardAssemblyReferences != null)
				{
					session.AssemblyDirectives.AddRange(standardAssemblyReferences);
				}
				if (session.IsHostSpecific)
				{
					session.AssemblyDirectives.Add(typeof(ITextTemplatingEngineHost).Assembly.Location);
					session.AssemblyDirectives.Add(typeof(ServiceProviderExtensions).Assembly.Location);
				}
			}
			catch (Exception ex3)
			{
				if (IsCriticalException(ex3))
				{
					throw;
				}
				scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex3);
				LogError(session.TemplateFile, -1, -1, Resources.ExceptionGettingStandardReferences + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex3), false);
			}
			if (stringBuilder2.Length != 0 || stringBuilder3.Length != 0)
			{
				AddInitializeMethod(session, stringBuilder, stringBuilder2, stringBuilder3);
			}
			if (stringBuilder.Length != 0)
			{
				Block block = new Block(BlockType.ClassFeature, stringBuilder.ToString());
				block.FileName = session.TemplateFile;
				blocks.Add(block);
			}
			if (codeAttributeDeclarationCollection.Count > 0)
			{
				CodeAttributeArgumentComparer.Instance.Provider = session.CodeDomProvider;
				templateClassCustomAttributes = new CodeAttributeDeclarationCollection(codeAttributeDeclarationCollection.OfType<CodeAttributeDeclaration>().Distinct(CodeAttributeDeclarationComparer.Instance).ToArray());
			}
			else
			{
				templateClassCustomAttributes = new CodeAttributeDeclarationCollection();
			}
		}

		/// <summary>
		/// Add an override to the Initialize method to contribute preInit or postInit code
		/// </summary>
		/// <param name="session"></param>
		/// <param name="directivesBuilder"></param>
		/// <param name="preInitBuilder"></param>
		/// <param name="postInitBuilder"></param>
		private static void AddInitializeMethod(TemplateProcessingSession session, StringBuilder directivesBuilder, StringBuilder preInitBuilder, StringBuilder postInitBuilder)
		{
			StringBuilder stringBuilder = new StringBuilder();
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod
			{
				Name = "Initialize"
			};
			MemberAttributes memberAttributes = ProvideBaseClassOverrideAttribute(session);
			codeMemberMethod.Attributes = (memberAttributes | MemberAttributes.Public);
			codeMemberMethod.Comments.AddSummaryComment("Initialize the template");
			if (preInitBuilder.Length != 0)
			{
				CodeSnippetStatement value = new CodeSnippetStatement(preInitBuilder.ToString());
				codeMemberMethod.Statements.Add(value);
			}
			if ((memberAttributes & MemberAttributes.Override) == MemberAttributes.Override)
			{
				codeMemberMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), codeMemberMethod.Name));
			}
			if (postInitBuilder.Length != 0)
			{
				CodeConditionStatement value2 = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Errors"), "HasErrors"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false)), new CodeSnippetStatement(postInitBuilder.ToString()));
				codeMemberMethod.Statements.Add(value2);
			}
			CodeGeneratorOptions options = new CodeGeneratorOptions
			{
				BlankLinesBetweenMembers = true,
				IndentString = "    ",
				VerbatimOrder = true,
				BracingStyle = "C"
			};
			using (StringWriter writer = new StringWriter(stringBuilder, session.FormatProvider))
			{
				session.CodeDomProvider.GenerateCodeFromMember(codeMemberMethod, writer, options);
			}
			directivesBuilder.AppendLine(stringBuilder.ToString());
		}

		/// <summary>
		/// Provide an attribute specifying whether optional methods from the base class should be overridden or virtual
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>
		private static MemberAttributes ProvideBaseClassOverrideAttribute(TemplateProcessingSession session)
		{
			if (!session.Preprocess || !string.IsNullOrEmpty(session.BaseClassName))
			{
				return MemberAttributes.Override;
			}
			return (MemberAttributes)0;
		}

		private IEnumerable<Directive> ProcessBuiltInDirectives(List<Block> blocks, ITextTemplatingEngineHost host, TemplateProcessingSession session, ITelemetryScope scope)
		{
			List<Directive> list = new List<Directive>();
			VisitedFiles includedFiles = new VisitedFiles();
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (block.Type != 0)
				{
					continue;
				}
				while (session.IncludeStack.Count > 0 && StringComparer.OrdinalIgnoreCase.Compare(session.IncludeStack.Peek(), block.FileName) != 0)
				{
					session.IncludeStack.Pop();
				}
				Directive directive = TemplateParser.ParseDirectiveBlock(block, errors);
				if (directive == null)
				{
					continue;
				}
				if (IsBuiltInDirective(directive))
				{
					List<Block> list2 = ProcessBuiltInDirective(directive, host, session, includedFiles, scope);
					if (list2 == null || list2.Count == 0)
					{
						continue;
					}
					if (session.IncludeStack.Contains(list2[0].FileName))
					{
						LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.RecursiveInclude, list2[0].FileName), true);
						continue;
					}
					session.IncludeStack.Push(list2[0].FileName);
					int j;
					for (j = 0; j < list2.Count && list2[j].Type != BlockType.ClassFeature; j++)
					{
					}
					blocks.InsertRange(i + 1, list2.GetRange(0, j));
					if (j < list2.Count)
					{
						blocks.AddRange(list2.GetRange(j, list2.Count - j));
					}
				}
				else
				{
					list.Add(directive);
				}
			}
			return list;
		}

		/// <summary>
		/// Process all directives handled by either extension directive processors or non built-in directives that ship with the engine.
		/// </summary>
		/// <remarks>
		/// The difference between built-in directives and custom but shipped directives is that true built-in directives are allowed to mess with the processing logic.
		/// Custom shipped directives have the same restrictions as end-user extension directives.  This is subtle but helps with keeping testing down as we know when they fire in the cycle.
		/// The difference should not be apparent to end users however.
		/// </remarks>
		private Dictionary<string, IDirectiveProcessor> ProcessCustomDirectives(ITextTemplatingEngineHost host, TemplateProcessingSession session, ITelemetryScope scope, IEnumerable<Directive> directivesToBeProcessed)
		{
			Dictionary<string, IDirectiveProcessor> dictionary = new Dictionary<string, IDirectiveProcessor>(StringComparer.OrdinalIgnoreCase);
			Queue<Tuple<Directive, IDirectiveProcessor>> queue = new Queue<Tuple<Directive, IDirectiveProcessor>>();
			scope.EndEvent.SetProperty(T4TelemetryProperty.CustomDirectivesProperty, directivesToBeProcessed.Any());
			foreach (Directive item3 in directivesToBeProcessed)
			{
				Type type = GetLocalCustomDirectiveProcessor(item3);
				if (string.IsNullOrEmpty(item3.DirectiveProcessorName))
				{
					LogError(item3.Block, string.Format(CultureInfo.CurrentCulture, Resources.NoProcessorForDirective, item3.DirectiveName), false);
					continue;
				}
				if (!dictionary.TryGetValue(item3.DirectiveProcessorName, out IDirectiveProcessor value))
				{
					try
					{
						if (type == null)
						{
							type = host.ResolveDirectiveProcessor(item3.DirectiveProcessorName);
						}
						if (type == null)
						{
							LogError(item3.Block, string.Format(CultureInfo.CurrentCulture, Resources.NoProcessorTypeForDirective, item3.DirectiveProcessorName, item3.DirectiveName), false);
						}
						else
						{
							if (typeof(IDirectiveProcessor).IsAssignableFrom(type))
							{
								goto IL_017d;
							}
							LogError(item3.Block, string.Format(CultureInfo.CurrentCulture, Resources.IncorrectDPType, type, item3.DirectiveProcessorName), false);
						}
					}
					catch (Exception ex)
					{
						if (IsCriticalException(ex))
						{
							throw;
						}
						scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex);
						LogError(item3.Block, string.Format(CultureInfo.CurrentCulture, Resources.NoProcessorTypeForDirective, item3.DirectiveProcessorName, item3.DirectiveName) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
					}
					continue;
				}
				goto IL_0267;
				IL_017d:
				try
				{
					value = (Activator.CreateInstance(type) as IDirectiveProcessor);
					if (value != null)
					{
						value.Initialize(host);
						IRecognizeHostSpecific recognizeHostSpecific = value as IRecognizeHostSpecific;
						if (recognizeHostSpecific != null)
						{
							if (recognizeHostSpecific.RequiresProcessingRunIsHostSpecific)
							{
								session.HostSpecific = HostSpecific.True;
							}
						}
						else if (value.RequiresProcessingRunIsHostSpecific)
						{
							session.HostSpecific = HostSpecific.True;
						}
						value.StartProcessingRun(session.CodeDomProvider, session.TemplateContents, errors);
						dictionary[item3.DirectiveProcessorName] = value;
						goto IL_0267;
					}
					LogError(item3.Block, string.Format(CultureInfo.CurrentCulture, Resources.CannotInitializeProcessor, type.ToString(), item3.DirectiveName), false);
				}
				catch (Exception ex2)
				{
					if (IsCriticalException(ex2))
					{
						throw;
					}
					LogError(item3.Block, string.Format(CultureInfo.CurrentCulture, Resources.CannotInitializeProcessor, type, item3.DirectiveName) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex2), false);
				}
				continue;
				IL_0267:
				if (value != null)
				{
					queue.Enqueue(new Tuple<Directive, IDirectiveProcessor>(item3, value));
				}
			}
			while (queue.Count > 0)
			{
				Tuple<Directive, IDirectiveProcessor> tuple = queue.Dequeue();
				Directive item = tuple.Item1;
				IDirectiveProcessor item2 = tuple.Item2;
				IRecognizeHostSpecific recognizeHostSpecific2 = item2 as IRecognizeHostSpecific;
				if (recognizeHostSpecific2 != null)
				{
					recognizeHostSpecific2.SetProcessingRunIsHostSpecific(session.IsHostSpecific);
				}
				else
				{
					item2.SetProcessingRunIsHostSpecific(session.IsHostSpecific);
				}
				try
				{
					if (item2.IsDirectiveSupported(item.DirectiveName))
					{
						item2.ProcessDirective(item.DirectiveName, item.Parameters);
					}
					else
					{
						LogError(item.Block, string.Format(CultureInfo.CurrentCulture, Resources.ProcessorNotSupportDirective, item.DirectiveProcessorName, item.DirectiveName), false);
					}
				}
				catch (Exception ex3)
				{
					if (IsCriticalException(ex3))
					{
						throw;
					}
					LogError(item.Block, string.Format(CultureInfo.CurrentCulture, Resources.ExceptionProcessingDirective, item.DirectiveName) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex3), false);
				}
			}
			return dictionary;
		}

		/// <summary>
		/// Get the type for any directive that is implemented in this assembly.
		/// </summary>
		/// <remarks>
		/// Amend the directive object to have an appropriate processor name.
		/// </remarks>
		private static Type GetLocalCustomDirectiveProcessor(Directive directive)
		{
			if (string.IsNullOrEmpty(directive.DirectiveProcessorName) && StringComparer.OrdinalIgnoreCase.Compare("parameter", directive.DirectiveName) == 0)
			{
				directive.SetDirectiveProcessorName("ParameterDirectiveProcessor");
				return typeof(ParameterDirectiveProcessor);
			}
			return null;
		}

		/// <summary>
		/// Says whether a given directive is built-in (handled by the engine) or should
		/// be handled by a custom directive processor
		/// </summary>
		private static bool IsBuiltInDirective(Directive directive)
		{
			if (string.Compare(directive.DirectiveName, "include", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(directive.DirectiveName, "assembly", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(directive.DirectiveName, "import", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(directive.DirectiveName, "template", StringComparison.OrdinalIgnoreCase) != 0)
			{
				return string.Compare(directive.DirectiveName, "output", StringComparison.OrdinalIgnoreCase) == 0;
			}
			return true;
		}

		/// <summary>
		/// Process an include directive by asking the host to read the included file, and
		/// parsing the contents into blocks.
		/// </summary>
		private List<Block> ProcessIncludeDirective(Directive directive, ITextTemplatingEngineHost host, VisitedFiles includedFiles, ITelemetryScope scope)
		{
			bool result = false;
			if (directive.Parameters.TryGetValue("file", out string value))
			{
				if (directive.Parameters.TryGetValue("once", out string value2))
				{
					bool.TryParse(value2, out result);
				}
				try
				{
					value = Environment.ExpandEnvironmentVariables(value);
					string content;
					string location;
					bool flag = host.LoadIncludeText(value, out content, out location);
					if (!flag && !Path.IsPathRooted(value) && !string.IsNullOrEmpty(directive.Block.FileName))
					{
						string requestFileName = Path.Combine(Path.GetDirectoryName(directive.Block.FileName), value);
						flag = host.LoadIncludeText(requestFileName, out content, out location);
					}
					if (flag && !string.IsNullOrEmpty(content))
					{
						bool flag2 = includedFiles.Visit(string.IsNullOrEmpty(location) ? value : Path.GetFullPath(location));
						if (result && flag2)
						{
							return Enumerable.Empty<Block>().ToList();
						}
						return TemplateParser.ParseTemplateIntoBlocks(content, location, errors);
					}
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.BlankIncludeFile, value), false);
				}
				catch (Exception ex)
				{
					if (IsCriticalException(ex))
					{
						throw;
					}
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.ErrorLoadingIncludeFile, value) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
					scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex);
				}
			}
			else
			{
				LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.NotEnoughDirectiveParameters, "file", directive.DirectiveName), false);
			}
			return new List<Block>(0);
		}

		/// <summary>
		/// Process the assembly directive by adding the assembly reference to the list of
		/// assembly references maintained by the TemplateProcessingSession
		/// </summary>
		private void ProcessAssemblyDirective(Directive directive, TemplateProcessingSession session)
		{
			if (directive.Parameters.TryGetValue("name", out string value))
			{
				session.AssemblyDirectives.Add(value);
			}
			else
			{
				LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.NotEnoughDirectiveParameters, "name", directive.DirectiveName), false);
			}
		}

		/// <summary>
		/// Process an import directive by adding the namespace to the list of namespaces
		/// maintained by the TemplateProcessingSession
		/// </summary>
		/// <param name="directive"></param>
		/// <param name="session"></param>
		private void ProcessImportDirective(Directive directive, TemplateProcessingSession session)
		{
			if (directive.Parameters.TryGetValue("namespace", out string value))
			{
				session.ImportDirectives.Add(value);
			}
			else
			{
				LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.NotEnoughDirectiveParameters, "namespace", directive.DirectiveName), false);
			}
		}

		private bool IsSupportedLanguage(Directive directive, string languageParameterFromDirective, string expectedLangauge, string notSupportedLanguage)
		{
			if (languageParameterFromDirective.StartsWith(expectedLangauge, StringComparison.OrdinalIgnoreCase))
			{
				if (StringComparer.OrdinalIgnoreCase.Compare(languageParameterFromDirective, expectedLangauge) == 0)
				{
					return true;
				}
				if (StringComparer.OrdinalIgnoreCase.Compare(languageParameterFromDirective, notSupportedLanguage) == 0)
				{
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.CompilerVersionNotSupported, expectedLangauge, languageParameterFromDirective.Substring(expectedLangauge.Length)), true);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Process the template directive
		/// </summary>
		/// <param name="directive"></param>
		/// <param name="session"></param>
		private void ProcessTemplateDirective(Directive directive, TemplateProcessingSession session, ITelemetryScope scope)
		{
			if (directive.Parameters.TryGetValue("language", out string value))
			{
				if (IsSupportedLanguage(directive, value, "VB", "VBv3.5"))
				{
					session.Language = SupportedLanguage.VB;
					scope.EndEvent.SetProperty(T4TelemetryProperty.LanguageProperty, "vb");
				}
				else if (IsSupportedLanguage(directive, value, "C#", "C#v3.5"))
				{
					session.Language = SupportedLanguage.CSharp;
					scope.EndEvent.SetProperty(T4TelemetryProperty.LanguageProperty, "csharp");
				}
				else
				{
					scope.EndEvent.SetProperty(T4TelemetryProperty.LanguageProperty, "csharp");
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.InvalidLanguage, value), true);
				}
			}
			if (directive.Parameters.TryGetValue("inherits", out string value2))
			{
				session.BaseClassName = value2;
			}
			if (directive.Parameters.TryGetValue("culture", out string value3))
			{
				try
				{
					CultureInfo cultureInfo = new CultureInfo(value3);
					if (cultureInfo.IsNeutralCulture)
					{
						LogError(directive.Block, Resources.InvalidNeutralCulture, true);
						session.FormatProvider = CultureInfo.InvariantCulture;
					}
					else
					{
						session.FormatProvider = cultureInfo;
					}
				}
				catch (Exception e)
				{
					if (IsCriticalException(e))
					{
						throw;
					}
					LogError(directive.Block, Resources.InvalidCulture, true);
					session.FormatProvider = CultureInfo.InvariantCulture;
				}
			}
			if (directive.Parameters.TryGetValue("debug", out string value4))
			{
				if (bool.TryParse(value4, out bool result))
				{
					session.Debug = result;
				}
				else
				{
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.InvalidDebugParam, value4), true);
				}
			}
			if (directive.Parameters.TryGetValue("hostspecific", out string value5))
			{
				bool flag = false;
				if (bool.TryParse(value5, out bool result2))
				{
					session.HostSpecific = (result2 ? HostSpecific.True : HostSpecific.False);
					flag = true;
				}
				else if (StringComparer.OrdinalIgnoreCase.Compare(value5, HostSpecific.TrueFromBase.ToString()) == 0)
				{
					session.HostSpecific = HostSpecific.TrueFromBase;
					flag = true;
					if (string.IsNullOrEmpty(session.BaseClassName))
					{
						LogError(directive.Block, Resources.MissingBaseClass, false);
					}
				}
				if (!flag)
				{
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.InvalidHostSpecificParam, value5), true);
				}
			}
			if (directive.Parameters.TryGetValue("compilerOptions", out string value6))
			{
				if (File.Exists(value6) || Directory.Exists(value6))
				{
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.InvalidCompilerOption, value6), false);
				}
				session.CompilerOptions = value6;
			}
			if (directive.Parameters.TryGetValue("visibility", out string value7))
			{
				if (string.Equals(value7, "public", StringComparison.OrdinalIgnoreCase))
				{
					session.IsPublic = true;
				}
				else if (string.Equals(value7, "internal", StringComparison.OrdinalIgnoreCase))
				{
					session.IsPublic = false;
				}
				else
				{
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.InvalidVisibility, value7), true);
					session.IsPublic = true;
				}
			}
			if (directive.Parameters.TryGetValue("linePragmas", out string value8))
			{
				if (bool.TryParse(value8, out bool result3))
				{
					session.LinePragmas = result3;
				}
				else
				{
					LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.InvalidLinePragmasParam, value8), true);
				}
			}
		}

		/// <summary>
		/// Process the output directive to get the extension of the output file
		/// </summary>
		/// <param name="directive"></param>
		/// <param name="host"></param>
		private void ProcessOutputDirective(Directive directive, ITextTemplatingEngineHost host, ITelemetryScope scope)
		{
			if (directive.Parameters.TryGetValue("extension", out string value))
			{
				try
				{
					value = value.Trim();
					if (!value.StartsWith(".", StringComparison.OrdinalIgnoreCase))
					{
						value = "." + value;
					}
					host.SetFileExtension(value);
				}
				catch (Exception ex)
				{
					if (IsCriticalException(ex))
					{
						throw;
					}
					LogError(directive.Block, Resources.ExceptionSettingExtension + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), true);
					scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex);
				}
			}
			if (directive.Parameters.TryGetValue("encoding", out string value2) && !string.IsNullOrEmpty(value2))
			{
				Encoding encoding = null;
				value2 = value2.Trim();
				if (int.TryParse(value2, out int result))
				{
					try
					{
						encoding = Encoding.GetEncoding(result);
					}
					catch (Exception ex2)
					{
						if (IsCriticalException(ex2))
						{
							throw;
						}
						LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.EncodingIntegerFailed, result) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex2), true);
					}
				}
				else
				{
					try
					{
						encoding = Encoding.GetEncoding(value2);
					}
					catch (Exception ex3)
					{
						if (IsCriticalException(ex3))
						{
							throw;
						}
						LogError(directive.Block, string.Format(CultureInfo.CurrentCulture, Resources.EncodingStringFailed, value2) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex3), true);
					}
				}
				if (encoding != null)
				{
					try
					{
						host.SetOutputEncoding(encoding, true);
					}
					catch (Exception ex4)
					{
						if (IsCriticalException(ex4))
						{
							throw;
						}
						LogError(directive.Block, Resources.EncodingStringFailed + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex4), true);
						scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex4);
					}
				}
			}
		}

		private List<Block> ProcessBuiltInDirective(Directive directive, ITextTemplatingEngineHost host, TemplateProcessingSession session, VisitedFiles includedFiles, ITelemetryScope scope)
		{
			if (string.Compare(directive.DirectiveName, "include", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ProcessIncludeDirective(directive, host, includedFiles, scope);
			}
			if (string.Compare(directive.DirectiveName, "assembly", StringComparison.OrdinalIgnoreCase) == 0)
			{
				ProcessAssemblyDirective(directive, session);
			}
			else if (string.Compare(directive.DirectiveName, "import", StringComparison.OrdinalIgnoreCase) == 0)
			{
				ProcessImportDirective(directive, session);
			}
			else if (string.Compare(directive.DirectiveName, "template", StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (!session.ProcessedTemplateDirective)
				{
					ProcessTemplateDirective(directive, session, scope);
					session.ProcessedTemplateDirective = true;
				}
				else
				{
					LogError(directive.Block, Resources.MultipleTemplateDirectives, true);
				}
			}
			else if (string.Compare(directive.DirectiveName, "output", StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (!session.ProcessedOutputDirective)
				{
					ProcessOutputDirective(directive, host, scope);
					session.ProcessedOutputDirective = true;
				}
				else
				{
					LogError(directive.Block, Resources.MultipleOutputDirectives, true);
				}
			}
			return new List<Block>(0);
		}

		/// <summary>
		/// Constructs code for the generated transformation class using the CodeDomProvider
		/// for the template language.
		/// </summary>
		private string ConstructGeneratorCode(ITextTemplatingEngineHost host, List<Block> blocks, TemplateProcessingSession session, bool insertLineNumbers, string className, string namespaceName, CodeAttributeDeclarationCollection templateClassCustomAttributes, ITelemetryScope scope)
		{
			if (string.IsNullOrEmpty(className))
			{
				className = "GeneratedTextTransformation";
			}
			if (string.IsNullOrEmpty(namespaceName) && !session.Preprocess)
			{
				namespaceName = "Coding4Fun.VisualStudio.TextTemplating";
				namespaceName = CreateUniqueNamespaceName(session, namespaceName, host, blocks);
			}
			session.ClassFullName = namespaceName + "." + className;
			CodeNamespace codeNamespace = new CodeNamespace(namespaceName);
			if (session.Preprocess)
			{
				AddAutoGeneratedComment(codeNamespace);
			}
			foreach (string item in session.ImportDirectives.Where((string importString) => !string.IsNullOrEmpty(importString)))
			{
				codeNamespace.Imports.Add(new CodeNamespaceImport(item));
			}
			CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(className);
			if (session.Preprocess)
			{
				AddGeneratedCodeAttribute(codeTypeDeclaration);
				SetTypeVisibility(session, codeTypeDeclaration);
			}
			codeTypeDeclaration.CustomAttributes.AddRange(templateClassCustomAttributes);
			codeTypeDeclaration.Comments.AddSummaryComment("Class to produce the template output");
			codeTypeDeclaration.IsClass = true;
			codeTypeDeclaration.IsPartial = session.Preprocess;
			codeNamespace.Types.Add(codeTypeDeclaration);
			ConstructBaseClassOptions(session, codeTypeDeclaration, className, out CodeTypeDeclaration baseClass);
			if (session.Preprocess && baseClass != null)
			{
				AddGeneratedCodeAttribute(baseClass);
				SetTypeVisibility(session, baseClass);
				codeNamespace.Types.Add(baseClass);
				baseClass.Members.AddRange(ToStringHelper.ProvideHelpers(session.FormatProvider));
			}
			if (!string.IsNullOrEmpty(session.TemplateFile) && session.LinePragmas && session.Language == SupportedLanguage.CSharp)
			{
				codeTypeDeclaration.LinePragma = new CodeLinePragma(session.TemplateFile, 1);
				codeTypeDeclaration.Members.Add(new CodeSnippetTypeMember("#line hidden"));
			}
			ConstructTransformTextMethod(session, blocks, insertLineNumbers, codeTypeDeclaration);
			if (session.HostSpecific == HostSpecific.True)
			{
				AddHostProperty(codeTypeDeclaration);
			}
			CodeGeneratorOptions options = new CodeGeneratorOptions
			{
				VerbatimOrder = true,
				BlankLinesBetweenMembers = false,
				BracingStyle = "C"
			};
			bool firstClassFeatureFound = false;
			foreach (Block block in blocks)
			{
				firstClassFeatureFound = GenerateMemberForBlock(session, block, codeTypeDeclaration, insertLineNumbers, options, firstClassFeatureFound);
			}
			CodeDomProvider codeDomProvider = session.CodeDomProvider;
			using (StringWriter stringWriter = new StringWriter(session.FormatProvider))
			{
				try
				{
					codeDomProvider.GenerateCodeFromNamespace(codeNamespace, new IndentedTextWriter(stringWriter), options);
				}
				catch (Exception ex)
				{
					if (IsCriticalException(ex))
					{
						throw;
					}
					LogError(session.TemplateFile, -1, -1, Resources.ErrorGeneratingTranformationClass + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
					scope.PostFault(T4TelemetryEvent.TransformationFault, null, ex);
				}
				return stringWriter.ToString();
			}
		}

		/// <summary>
		/// Set the visibility of the type to public or internal
		/// </summary>
		/// <param name="session">The session to take the public/internal data from</param>
		/// <param name="typeToChange">The type to amend</param>
		private static void SetTypeVisibility(TemplateProcessingSession session, CodeTypeDeclaration typeToChange)
		{
			if (session.IsPublic)
			{
				typeToChange.TypeAttributes = ((typeToChange.TypeAttributes & ~TypeAttributes.VisibilityMask) | TypeAttributes.Public);
			}
			else
			{
				typeToChange.TypeAttributes = ((typeToChange.TypeAttributes & ~TypeAttributes.VisibilityMask) | TypeAttributes.NotPublic);
			}
		}

		/// <summary>
		/// Construct the TransformText method
		/// </summary>
		/// <param name="session"></param>
		/// <param name="blocks"></param>
		/// <param name="insertLineNumbers"></param>
		/// <param name="generatorType"></param>
		private static void ConstructTransformTextMethod(TemplateProcessingSession session, IEnumerable<Block> blocks, bool insertLineNumbers, CodeTypeDeclaration generatorType)
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			generatorType.Members.Add(codeMemberMethod);
			codeMemberMethod.Name = "TransformText";
			codeMemberMethod.Attributes = (ProvideBaseClassOverrideAttribute(session) | MemberAttributes.Public);
			codeMemberMethod.ReturnType = new CodeTypeReference(typeof(string));
			codeMemberMethod.Statements.AddRange(ConstructStatementsForGeneratorMethod(blocks, session, insertLineNumbers));
			codeMemberMethod.Comments.AddSummaryComment("Create the template output");
		}

		/// <summary>
		/// Add a comment to the class in the standard format to specify that it is autogenerated.
		/// </summary>
		/// <param name="codeNamespace"></param>
		private void AddAutoGeneratedComment(CodeNamespace codeNamespace)
		{
			codeNamespace.Comments.Add(new CodeCommentStatement("------------------------------------------------------------------------------"));
			codeNamespace.Comments.Add(new CodeCommentStatement("<auto-generated>"));
			codeNamespace.Comments.Add(new CodeCommentStatement("    " + Resources.AutoGenCommentLine1));
			codeNamespace.Comments.Add(new CodeCommentStatement("    " + string.Format(CultureInfo.InvariantCulture, Resources.AutoGenCommentLine2, GetType().Assembly.GetName().Version)));
			codeNamespace.Comments.Add(new CodeCommentStatement(" "));
			codeNamespace.Comments.Add(new CodeCommentStatement("    " + Resources.AutoGenCommentLine3));
			codeNamespace.Comments.Add(new CodeCommentStatement("    " + Resources.AutoGenCommentLine4));
			codeNamespace.Comments.Add(new CodeCommentStatement("</auto-generated>"));
			codeNamespace.Comments.Add(new CodeCommentStatement("------------------------------------------------------------------------------"));
		}

		/// <summary>
		/// Add a generated code marker attribute to the given class declaration
		/// </summary>
		/// <param name="generatorType"></param>
		private void AddGeneratedCodeAttribute(CodeTypeDeclaration generatorType)
		{
			CodeTypeReference codeTypeReference = new CodeTypeReference(typeof(GeneratedCodeAttribute));
			codeTypeReference.Options = CodeTypeReferenceOptions.GlobalReference;
			generatorType.CustomAttributes.Add(new CodeAttributeDeclaration(codeTypeReference, new CodeAttributeArgument(new CodePrimitiveExpression("Coding4Fun.VisualStudio.TextTemplating")), new CodeAttributeArgument(new CodePrimitiveExpression(GetType().Assembly.GetName().Version.ToString()))));
		}

		/// <summary>
		/// Set up the base class for the template
		/// </summary>
		/// <param name="session"></param>
		/// <param name="generatorType"></param>
		private static void ConstructBaseClassOptions(TemplateProcessingSession session, CodeTypeDeclaration generatorType, string className, out CodeTypeDeclaration baseClass)
		{
			baseClass = null;
			if (!string.IsNullOrEmpty(session.BaseClassName))
			{
				generatorType.BaseTypes.Add(new CodeTypeReference(session.BaseClassName));
				return;
			}
			if (!session.Preprocess)
			{
				generatorType.BaseTypes.Add(new CodeTypeReference(typeof(TextTransformation)));
				return;
			}
			string text = className.Trim() + "Base";
			baseClass = TextTransformation.ProvideBaseClass(text);
			generatorType.BaseTypes.Add(new CodeTypeReference(text));
		}

		/// <summary>
		/// Create a new namespace name to ensure the generated class has a unique identity
		/// </summary>
		/// <param name="session"></param>
		/// <param name="baseNamespaceName"></param>
		/// <param name="host"></param>
		/// <param name="blocks"></param>
		/// <returns></returns>
		private static string CreateUniqueNamespaceName(TemplateProcessingSession session, string baseNamespaceName, ITextTemplatingEngineHost host, List<Block> blocks)
		{
			object obj = null;
			try
			{
				obj = host.GetHostOption("CacheAssemblies");
			}
			catch (Exception e)
			{
				if (IsCriticalException(e))
				{
					throw;
				}
			}
			session.CacheAssemblies = (obj != null && obj is bool && (bool)obj);
			if (session.CacheAssemblies)
			{
				StringBuilder textToHash = new StringBuilder();
				blocks.ForEach(delegate(Block block)
				{
					textToHash.AppendLine(block.Text);
				});
				byte[] bytes = Encoding.UTF8.GetBytes(textToHash.ToString());
				byte[] array = hasher.ComputeHash(bytes);
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < array.Length; i++)
				{
					stringBuilder.Append(array[i].ToString("X2", CultureInfo.InvariantCulture));
				}
				baseNamespaceName += stringBuilder.ToString();
			}
			else
			{
				baseNamespaceName += Guid.NewGuid().ToString("N");
			}
			return baseNamespaceName;
		}

		/// <summary>
		/// Create a snippet member for a given block
		/// </summary>
		/// <param name="session"></param>
		/// <param name="block"></param>
		/// <param name="generatorType"></param>
		/// <param name="insertLineNumbers"></param>
		/// <param name="options"></param>
		/// <param name="firstClassFeatureFound"></param>
		/// <returns></returns>
		private static bool GenerateMemberForBlock(TemplateProcessingSession session, Block block, CodeTypeDeclaration generatorType, bool insertLineNumbers, CodeGeneratorOptions options, bool firstClassFeatureFound)
		{
			CodeSnippetTypeMember codeSnippetTypeMember = null;
			if (block.Type == BlockType.ClassFeature)
			{
				firstClassFeatureFound = true;
				if (!string.IsNullOrEmpty(block.Text))
				{
					codeSnippetTypeMember = new CodeSnippetTypeMember(block.Text);
				}
			}
			else if (block.Type == BlockType.BoilerPlate && firstClassFeatureFound)
			{
				CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Write", new CodePrimitiveExpression(block.Text));
				CodeExpressionStatement statement = new CodeExpressionStatement(expression);
				using (StringWriter stringWriter = new StringWriter(session.FormatProvider))
				{
					session.CodeDomProvider.GenerateCodeFromStatement(statement, stringWriter, options);
					codeSnippetTypeMember = new CodeSnippetTypeMember(stringWriter.ToString());
				}
			}
			else if (block.Type == BlockType.Expression && firstClassFeatureFound)
			{
				CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression(GetToStringHelperReference(session), "ToStringWithCulture", new CodeArgumentReferenceExpression(block.Text.Trim()));
				CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Write", codeMethodInvokeExpression);
				CodeExpressionStatement statement2 = new CodeExpressionStatement(expression2);
				using (StringWriter stringWriter2 = new StringWriter(session.FormatProvider))
				{
					session.CodeDomProvider.GenerateCodeFromStatement(statement2, stringWriter2, options);
					codeSnippetTypeMember = new CodeSnippetTypeMember(stringWriter2.ToString());
				}
			}
			if (codeSnippetTypeMember != null)
			{
				if (insertLineNumbers)
				{
					AddTypeMemberWithLinePragma(session, generatorType, block, codeSnippetTypeMember);
				}
				else
				{
					generatorType.Members.Add(codeSnippetTypeMember);
				}
			}
			return firstClassFeatureFound;
		}

		/// <summary>
		/// Calculate the reference property or static class that embodies the ToStringHelper
		/// </summary>
		private static CodeExpression GetToStringHelperReference(TemplateProcessingSession session)
		{
			if (session.Preprocess)
			{
				return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ToStringHelper");
			}
			return new CodeTypeReferenceExpression(typeof(ToStringHelper).FullName);
		}

		private static void AddHostProperty(CodeTypeDeclaration generatorType)
		{
			CodeMemberField value = new CodeMemberField(typeof(ITextTemplatingEngineHost), "hostValue")
			{
				Type = 
				{
					Options = CodeTypeReferenceOptions.GlobalReference
				},
				Attributes = MemberAttributes.Private
			};
			generatorType.Members.Add(value);
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.Name = "Host";
			codeMemberProperty.Type = new CodeTypeReference(typeof(ITextTemplatingEngineHost), CodeTypeReferenceOptions.GlobalReference);
			codeMemberProperty.Attributes = MemberAttributes.Public;
			codeMemberProperty.HasGet = true;
			codeMemberProperty.HasSet = true;
			CodeMemberProperty codeMemberProperty2 = codeMemberProperty;
			codeMemberProperty2.Comments.AddSummaryComment("The current host for the text templating engine");
			codeMemberProperty2.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "hostValue")));
			codeMemberProperty2.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "hostValue"), new CodePropertySetValueReferenceExpression()));
			generatorType.Members.Add(codeMemberProperty2);
		}

		/// <summary>
		/// Construct CodeDom statements for the TransformText method override on the generated
		/// transformation class
		/// </summary>
		private static CodeStatementCollection ConstructStatementsForGeneratorMethod(IEnumerable<Block> blocks, TemplateProcessingSession session, bool insertLineNumbers)
		{
			CodeStatementCollection codeStatementCollection = new CodeStatementCollection();
			foreach (Block block in blocks)
			{
				if (block.Type == BlockType.ClassFeature)
				{
					break;
				}
				switch (block.Type)
				{
				case BlockType.Statement:
				{
					CodeSnippetStatement codeSnippetStatement = new CodeSnippetStatement(block.Text);
					if (insertLineNumbers)
					{
						AddStatementWithLinePragma(session, codeStatementCollection, block, codeSnippetStatement);
					}
					else
					{
						codeStatementCollection.Add(codeSnippetStatement);
					}
					break;
				}
				case BlockType.BoilerPlate:
					if (!string.IsNullOrEmpty(block.Text))
					{
						CodeMethodInvokeExpression value = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Write", new CodePrimitiveExpression(block.Text));
						codeStatementCollection.Add(value);
					}
					break;
				case BlockType.Expression:
				{
					CodeMethodInvokeExpression codeMethodInvokeExpression = new CodeMethodInvokeExpression(GetToStringHelperReference(session), "ToStringWithCulture", new CodeArgumentReferenceExpression(block.Text.Trim()));
					CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Write", codeMethodInvokeExpression);
					CodeExpressionStatement codeExpressionStatement = new CodeExpressionStatement(expression);
					if (insertLineNumbers)
					{
						AddStatementWithLinePragma(session, codeStatementCollection, block, codeExpressionStatement);
					}
					else
					{
						codeStatementCollection.Add(codeExpressionStatement);
					}
					break;
				}
				}
			}
			CodeStatementCollection codeStatementCollection2;
			if (session.Preprocess)
			{
				codeStatementCollection2 = codeStatementCollection;
			}
			else
			{
				codeStatementCollection2 = new CodeStatementCollection();
				CodeTryCatchFinallyStatement codeTryCatchFinallyStatement = new CodeTryCatchFinallyStatement();
				codeStatementCollection2.Add(codeTryCatchFinallyStatement);
				codeTryCatchFinallyStatement.TryStatements.AddRange(codeStatementCollection);
				CodeCatchClause codeCatchClause = new CodeCatchClause("e");
				codeTryCatchFinallyStatement.CatchClauses.Add(codeCatchClause);
				codeCatchClause.Statements.Add(new CodeIndexerExpression(new CodeVariableReferenceExpression("e").Prop("Data"), "TextTemplatingProgress".Prim()).Assign(new CodeThisReferenceExpression().Prop("GenerationEnvironment").Call("ToString")));
				codeCatchClause.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression("System.Exception", "Template runtime error".Prim(), new CodeVariableReferenceExpression("e"))));
			}
			codeStatementCollection2.Add(new CodeMethodReturnStatement(new CodeThisReferenceExpression().Prop("GenerationEnvironment").Call("ToString")));
			return codeStatementCollection2;
		}

		/// <summary>
		/// Compiles the transformation code and prepares in for running in the runFactory
		/// </summary>
		private IDebugTransformationRun CompileAndPrepareRun(string generatorCode, ITextTemplatingEngineHost host, TemplateProcessingSession session, IDebugTransformationRunFactory runFactory, ITelemetryScope scope)
		{
			TransformationRunner transformationRunner = null;
			bool flag = false;
			using (ITelemetryScope telemetryScope = scope.StartOperation(T4TelemetryEvent.CompilationOperation))
			{
				try
				{
					AssemblyResolver @object = new AssemblyResolver();
					try
					{
						transformationRunner = (runFactory.CreateTransformationRun(typeof(TransformationRunner), generatorCode, @object.AssemblyResolve) as TransformationRunner);
					}
					catch (Exception ex)
					{
						if (IsCriticalException(ex))
						{
							throw;
						}
						telemetryScope.PostFault(T4TelemetryEvent.TransformationFault, null, ex);
					}
					if (transformationRunner == null)
					{
						LogError(session.TemplateFile, -1, -1, Resources.NoAppDomain, false);
						return null;
					}
					if (transformationRunner != null)
					{
						if (transformationRunner.Errors.HasErrors)
						{
							return null;
						}
						ResolveAssemblyReferences(host, session);
						if (errors.HasErrors)
						{
							return null;
						}
						transformationRunner.PreLoadAssemblies(session.AssemblyDirectives.ToArray());
						ITextTemplatingEngineHost host2 = null;
						if (session.IsHostSpecific)
						{
							host2 = host;
						}
						try
						{
							flag = transformationRunner.PrepareTransformation(session, generatorCode, host2);
						}
						catch (SerializationException)
						{
							LogError(session.TemplateFile, -1, -1, Resources.SessionHostMarshalError, false);
							throw;
						}
					}
				}
				catch (AppDomainUnloadedException exception)
				{
					telemetryScope.PostFault(T4TelemetryEvent.TransformationFault, null, exception);
					LogError(session.TemplateFile, -1, -1, Resources.NoAppDomain, false);
				}
				catch (Exception ex3)
				{
					if (IsCriticalException(ex3))
					{
						throw;
					}
					telemetryScope.PostFault(T4TelemetryEvent.TransformationFault, null, ex3);
					LogError(session.TemplateFile, -1, -1, Resources.ExceptionWhileRunningCode + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex3), false);
				}
				finally
				{
					if (transformationRunner != null)
					{
						errors.AddRange(transformationRunner.Errors);
						transformationRunner.ClearErrors();
					}
				}
				if (flag)
				{
					return transformationRunner;
				}
				return null;
			}
		}

		/// <summary>
		/// Allow the host to process all assembly references in the session
		/// </summary>
		private void ResolveAssemblyReferences(ITextTemplatingEngineHost host, TemplateProcessingSession session)
		{
			if (session.UserTransformationSession != null)
			{
				IEnumerable<Type> source = CollateSessionTypes(session.UserTransformationSession);
				session.AssemblyDirectives.AddRange(source.Select((Type t) => (!t.Assembly.GlobalAssemblyCache) ? t.Assembly.Location : t.Assembly.FullName));
			}
			List<string> collection = new List<string>(session.AssemblyDirectives.Distinct());
			session.AssemblyDirectives.Clear();
			session.AssemblyDirectives.AddRange(collection);
			for (int i = 0; i < session.AssemblyDirectives.Count; i++)
			{
				try
				{
					session.AssemblyDirectives[i] = host.ResolveAssemblyReference(Environment.ExpandEnvironmentVariables(session.AssemblyDirectives[i]));
					if (session.AssemblyDirectives[i] == null)
					{
						session.AssemblyDirectives.RemoveAt(i);
						i--;
					}
				}
				catch (Exception ex)
				{
					if (IsCriticalException(ex))
					{
						throw;
					}
					LogError(session.TemplateFile, -1, -1, string.Format(CultureInfo.CurrentCulture, Resources.ExceptionResolvingAssembly, session.AssemblyDirectives[i]) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
				}
			}
		}

		/// <summary>
		/// Get the type of the session and any types within it's dictionary that are not in standard assemblies
		/// </summary>
		private static IEnumerable<Type> CollateSessionTypes(ITextTemplatingSession userTransformationSession)
		{
			List<Type> list = new List<Type>();
			Type type = userTransformationSession.GetType();
			if (NonStandardType(type))
			{
				list.Add(type);
			}
			foreach (object value in userTransformationSession.Values)
			{
				Type type2 = value.GetType();
				if (NonStandardType(type2))
				{
					list.Add(type2);
				}
			}
			return list;
		}

		/// <summary>
		/// Is the given type a type whose assembly will already be loaded into the transformation AppDomain?
		/// </summary>
		private static bool NonStandardType(Type candidate)
		{
			Assembly assembly = candidate.Assembly;
			if (!candidate.IsPrimitive)
			{
				return !assembly.GlobalAssemblyCache;
			}
			return false;
		}

		internal static bool IsCriticalException(Exception e)
		{
			if (e is StackOverflowException || e is OutOfMemoryException || e is ThreadAbortException)
			{
				return true;
			}
			if (e.InnerException != null)
			{
				return IsCriticalException(e.InnerException);
			}
			return false;
		}

		/// <summary>
		/// Add a typemember code snippet to a type with the correct line pragma decorating it.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="generatorType"></param>
		/// <param name="block"></param>
		/// <param name="member"></param>
		private static void AddTypeMemberWithLinePragma(TemplateProcessingSession session, CodeTypeDeclaration generatorType, Block block, CodeSnippetTypeMember member)
		{
			bool flag = string.IsNullOrEmpty(block.FileName) && session.Language == SupportedLanguage.CSharp;
			if (session.LinePragmas)
			{
				int lineNumber = (block.StartLineNumber <= 0) ? 1 : block.StartLineNumber;
				if (flag)
				{
					generatorType.Members.Add(new CodeSnippetTypeMember("#line " + lineNumber.ToString()));
				}
				else
				{
					member.LinePragma = new CodeLinePragma(block.FileName, lineNumber);
				}
			}
			generatorType.Members.Add(member);
			if (session.LinePragmas && flag)
			{
				generatorType.Members.Add(new CodeSnippetTypeMember("#line default"));
			}
		}

		/// <summary>
		/// Add a statement code snippet to a list of statements with the correct line pragma decorating it.
		/// </summary>
		private static void AddStatementWithLinePragma(TemplateProcessingSession session, CodeStatementCollection statements, Block block, CodeStatement statement)
		{
			int lineNumber = (block.StartLineNumber <= 0) ? 1 : block.StartLineNumber;
			AddStatementWithLinePragma(session, statements, lineNumber, block.FileName, statement);
		}

		/// <summary>
		/// Add a statement code snippet to a list of statements with the correct line pragma decorating it.
		/// </summary>
		private static void AddStatementWithLinePragma(TemplateProcessingSession session, CodeStatementCollection statements, int lineNumber, string fileName, CodeStatement statement)
		{
			bool flag = string.IsNullOrEmpty(fileName) && session.Language == SupportedLanguage.CSharp;
			if (session.LinePragmas)
			{
				if (flag)
				{
					statements.Add(new CodeSnippetStatement("#line " + lineNumber.ToString()));
				}
				else
				{
					statement.LinePragma = new CodeLinePragma(fileName, lineNumber);
				}
			}
			statements.Add(statement);
			if (session.LinePragmas && flag)
			{
				statements.Add(new CodeSnippetStatement("#line default"));
			}
		}
	}
}
