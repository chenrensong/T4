using Coding4Fun.VisualStudio.TextTemplating.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Base class for a directive processor that follows the requires, provides pattern.
	/// </summary>
	public abstract class RequiresProvidesDirectiveProcessor : DirectiveProcessor
	{
		private enum ProcessorState
		{
			PreStart,
			Started
		}

		private CodeDomProvider languageCodeDomProvider;

		/// <summary>
		/// Buffer that collates all of the code that instances of this directive processor contribute during a processing run
		/// </summary>
		private StringBuilder codeBuffer;

		/// <summary>
		/// Buffer that collates all of the code that instances of this directive processor need to run before base class initialization during a processing run
		/// </summary>
		private StringBuilder preInitializationBuffer;

		/// <summary>
		/// Buffer that collates all of the code that instances of this directive processor need to run after base class initialization during a processing run
		/// </summary>
		private StringBuilder postInitializationBuffer;

		private ProcessorState state;

		/// <summary>
		/// Gets associated text templating host.
		/// </summary>
		protected ITextTemplatingEngineHost Host
		{
			get;
			private set;
		}

		/// <summary>
		/// The friendly name of this processor
		/// </summary>
		protected abstract string FriendlyName
		{
			get;
		}

		/// <summary>
		/// Initializes the processors.
		/// </summary>
		/// <param name="host"></param>
		public override void Initialize(ITextTemplatingEngineHost host)
		{
			if (host == null)
			{
				throw new ArgumentNullException("host");
			}
			base.Initialize(host);
			state = ProcessorState.PreStart;
			Host = host;
		}

		/// <summary>
		/// Starts processing run.
		/// </summary>
		/// <param name="languageProvider">Target language provider.</param>
		/// <param name="templateContents">The contents of the template being processed</param>
		/// <param name="errors">colelction to report processing errors in</param>
		public override void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
		{
			if (languageProvider == null)
			{
				throw new ArgumentNullException("languageProvider");
			}
			base.StartProcessingRun(languageProvider, templateContents, errors);
			if (state != 0)
			{
				throw new InvalidOperationException(Resources.StartProcessingCallError);
			}
			state = ProcessorState.Started;
			languageCodeDomProvider = languageProvider;
			codeBuffer = new StringBuilder();
			preInitializationBuffer = new StringBuilder();
			postInitializationBuffer = new StringBuilder();
		}

		/// <summary>
		/// Provide a token to uniquely identify this instance of a directive processor
		/// </summary>
		/// <remarks>
		/// By default, allow an ID parameter to be used on the directive.
		/// Frequently, directive processors would choose to use one of their Provides parameters
		/// </remarks>
		/// <returns>A unique id for this directive instance</returns>
		protected virtual string ProvideUniqueId(string directiveName, IDictionary<string, string> arguments, IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments)
		{
			if (directiveName == null)
			{
				throw new ArgumentNullException("directiveName");
			}
			if (arguments == null)
			{
				throw new ArgumentNullException("arguments");
			}
			if (providesArguments == null)
			{
				throw new ArgumentNullException("providesArguments");
			}
			string text = ProcessIdArgument(arguments);
			if (string.IsNullOrEmpty(text))
			{
				if (providesArguments.Count > 0)
				{
					IEnumerator<string> enumerator = providesArguments.Keys.GetEnumerator();
					enumerator.MoveNext();
					text = providesArguments[enumerator.Current];
				}
				else
				{
					text = directiveName;
				}
			}
			return text;
		}

		/// <summary>
		/// Processes a single directive.
		/// </summary>
		/// <param name="directiveName">Directive name.</param>
		/// <param name="arguments">Directive arguments.</param>
		public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
		{
			if (directiveName == null)
			{
				throw new ArgumentNullException("directiveName");
			}
			if (arguments == null)
			{
				throw new ArgumentNullException("arguments");
			}
			if (state != ProcessorState.Started)
			{
				throw new InvalidOperationException(Resources.ProcessDirectiveCallError);
			}
			if (IsDirectiveSupported(directiveName))
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				InitializeProvidesDictionary(directiveName, dictionary);
				ProcessArgument(directiveName, arguments, "provides", dictionary, false);
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				InitializeRequiresDictionary(directiveName, dictionary2);
				Dictionary<string, string> dictionary3 = new Dictionary<string, string>(dictionary2);
				List<string> list = new List<string>(dictionary2.Keys);
				foreach (string item in list)
				{
					dictionary2[item] = null;
				}
				ProcessArgument(directiveName, arguments, "requires", dictionary2, true);
				string directiveId = ProvideUniqueId(directiveName, arguments, dictionary2, dictionary);
				List<string> list2 = new List<string>();
				foreach (string key in dictionary2.Keys)
				{
					if (dictionary2[key] == null)
					{
						list2.Add(key);
					}
				}
				foreach (string item2 in list2)
				{
					string value = Host.ResolveParameterValue(directiveId, FriendlyName, item2);
					if (string.IsNullOrEmpty(value))
					{
						if (dictionary3[item2] == null)
						{
							throw new DirectiveProcessorException(string.Format(CultureInfo.CurrentCulture, Resources.CannotResolveRequiresParameter, item2, directiveName));
						}
						dictionary2[item2] = dictionary3[item2];
					}
					else
					{
						dictionary2[item2] = value;
					}
				}
				PostProcessArguments(directiveName, dictionary2, dictionary);
				GeneratePreInitializationCode(directiveName, preInitializationBuffer, languageCodeDomProvider, dictionary2, dictionary);
				GeneratePostInitializationCode(directiveName, postInitializationBuffer, languageCodeDomProvider, dictionary2, dictionary);
				GenerateTransformCode(directiveName, codeBuffer, languageCodeDomProvider, dictionary2, dictionary);
				return;
			}
			throw new DirectiveProcessorException(string.Format(CultureInfo.CurrentCulture, Resources.DirectiveNotSupported, directiveName));
		}

		/// <summary>
		/// Finishes template processing.
		/// </summary>
		public override void FinishProcessingRun()
		{
			state = ProcessorState.PreStart;
		}

		/// <summary>
		/// Gets generated class code.
		/// </summary>
		/// <returns></returns>
		public override string GetClassCodeForProcessingRun()
		{
			if (state != 0)
			{
				throw new InvalidOperationException(Resources.GetClassCodeCallError);
			}
			return codeBuffer.ToString();
		}

		/// <summary>
		/// Get the code to contribute to the body of the initialize method of the generated
		/// template processing class as a consequence of the most recent run.
		/// This code will run before the base class' Initialize method
		/// </summary>
		/// <returns></returns>
		public override string GetPreInitializationCodeForProcessingRun()
		{
			if (state != 0)
			{
				throw new InvalidOperationException(Resources.GetClassCodeCallError);
			}
			return preInitializationBuffer.ToString();
		}

		/// <summary>
		/// Get the code to contribute to the body of the initialize method of the generated
		/// template processing class as a consequence of the most recent run.
		/// This code will run after the base class' Initialize method
		/// </summary>
		/// <returns></returns>
		public override string GetPostInitializationCodeForProcessingRun()
		{
			if (state != 0)
			{
				throw new InvalidOperationException(Resources.GetClassCodeCallError);
			}
			return postInitializationBuffer.ToString();
		}

		/// <summary>
		/// Gets list of importt.
		/// </summary>
		/// <returns></returns>
		public override string[] GetImportsForProcessingRun()
		{
			if (state != 0)
			{
				throw new InvalidOperationException(Resources.GetImportsCallError);
			}
			return new string[0];
		}

		/// <summary>
		/// Gets list of references.
		/// </summary>
		/// <returns></returns>
		public override string[] GetReferencesForProcessingRun()
		{
			if (state != 0)
			{
				throw new InvalidOperationException(Resources.GetReferencesCallError);
			}
			return new string[0];
		}

		/// <summary>
		/// Method for derived classes to make any modifications to the dictionaries that they require
		/// </summary>
		/// <param name="directiveName"></param>
		/// <param name="requiresArguments"></param>
		/// <param name="providesArguments"></param>
		protected virtual void PostProcessArguments(string directiveName, IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments)
		{
		}

		/// <summary>
		/// Method for derived classes to generate the code they wish to add to the TextTransformation generated class.
		/// </summary>
		/// <param name="directiveName"></param>
		/// <param name="codeBuffer"></param>
		/// <param name="languageProvider"></param>
		/// <param name="requiresArguments"></param>
		/// <param name="providesArguments"></param>
		protected abstract void GenerateTransformCode(string directiveName, StringBuilder codeBuffer, CodeDomProvider languageProvider, IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments);

		/// <summary>
		/// Method for derived classes to contribute additively to initialization code for the TextTransformation generated class.
		/// </summary>
		/// <remarks>
		/// Additive code is useful where there are multiple directive processor instances each needing to have some instance-specific initialization.
		/// As GenerateTransformCode can add methods, matching initialization code is often required to call those methods.
		/// This code will be added before the call to the base class.
		/// </remarks>
		/// <param name="directiveName"></param>
		/// <param name="codeBuffer"></param>
		/// <param name="languageProvider"></param>
		/// <param name="requiresArguments"></param>
		/// <param name="providesArguments"></param>
		protected abstract void GeneratePreInitializationCode(string directiveName, StringBuilder codeBuffer, CodeDomProvider languageProvider, IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments);

		/// <summary>
		/// Method for derived classes to contribute additively to initialization code for the TextTransformation generated class.
		/// </summary>
		/// <remarks>
		/// Additive code is useful where there are multiple directive processor instances each needing to have some instance-specific initialization.
		/// As GenerateTransformCode can add methods, matching initialization code is often required to call those methods.
		/// This code will be added after the call to the base class.
		/// </remarks>
		/// <param name="directiveName"></param>
		/// <param name="codeBuffer"></param>
		/// <param name="languageProvider"></param>
		/// <param name="requiresArguments"></param>
		/// <param name="providesArguments"></param>
		protected abstract void GeneratePostInitializationCode(string directiveName, StringBuilder codeBuffer, CodeDomProvider languageProvider, IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments);

		/// <summary>
		/// Method for derived classes to specify the requires arguments they need for each directive by putting "<null>" in the matching dictionary slot.</null>
		/// </summary>
		/// <param name="directiveName"></param>
		/// <param name="requiresDictionary"></param>
		protected abstract void InitializeRequiresDictionary(string directiveName, IDictionary<string, string> requiresDictionary);

		/// <summary>
		/// Method for derived classes to specify the provides parameters they will supply for each directive by putting the default name in the matching dictionary slot.
		/// </summary>
		/// <param name="directiveName"></param>
		/// <param name="providesDictionary"></param>
		protected abstract void InitializeProvidesDictionary(string directiveName, IDictionary<string, string> providesDictionary);

		/// <summary>
		/// Process an argument string consisting of parameter name value pairs formatted as below:
		/// name[=[']value[']][;name[=[']value[']]]
		/// </summary>
		private static void ProcessArgument(string directiveName, IDictionary<string, string> arguments, string argumentName, IDictionary<string, string> argumentDictionary, bool mandatory)
		{
			string value;
			bool flag = arguments.TryGetValue(argumentName, out value);
			if (mandatory && !flag)
			{
				throw new DirectiveProcessorException(string.Format(CultureInfo.CurrentCulture, Resources.NotEnoughDirectiveParameters, argumentName, directiveName));
			}
			if (flag)
			{
				IDictionary<string, string> dictionary = ParseArgument(value);
				foreach (string key in dictionary.Keys)
				{
					if (!argumentDictionary.ContainsKey(key))
					{
						throw new DirectiveProcessorException(string.Format(CultureInfo.CurrentCulture, Resources.UnsupportedArgumentValue, key, argumentName, directiveName));
					}
					string text = dictionary[key];
					if (!string.IsNullOrEmpty(text))
					{
						argumentDictionary[key] = StripQuotes(text);
					}
				}
			}
		}

		/// <summary>
		/// Return the id of the directive if it has one
		/// </summary>
		/// <returns>the id or string.Empty</returns>
		private static string ProcessIdArgument(IDictionary<string, string> arguments)
		{
			if (!arguments.TryGetValue("id", out string value))
			{
				return string.Empty;
			}
			return value;
		}

		private static string StripQuotes(string text)
		{
			return text.Trim().Trim('\'');
		}

		/// <summary>
		/// Parse a standardized argument string into a dictionary of argument name, value pairs
		/// </summary>
		/// <remarks>Values are optional, pairs are semicolon separated</remarks>
		private static IDictionary<string, string> ParseArgument(string argument)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string[] array = argument.Split(';');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.Contains("="))
				{
					string[] array3 = text.Split('=');
					if (array3.Length != 2)
					{
						throw new FormatException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidDirectiveArgumentFormat, text));
					}
					dictionary.Add(array3[0], array3[1]);
				}
				else
				{
					dictionary.Add(text, null);
				}
			}
			return dictionary;
		}
	}
}
