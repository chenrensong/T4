using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>Interface for a directive processor.</summary>
	public interface IDirectiveProcessor
	{
		/// <summary>Error collection for DirectiveProcessor to add errors/warnings to.</summary>
		/// <returns>Returns <see cref="T:System.CodeDom.Compiler.CompilerErrorCollection" />.</returns>
		CompilerErrorCollection Errors
		{
			get;
		}

		/// <summary>Allow a directive processor to specify that it needs the run to be host-specific.</summary>
		/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
		bool RequiresProcessingRunIsHostSpecific
		{
			get;
		}

		/// <summary>Initializes the processor instance.</summary>
		void Initialize(ITextTemplatingEngineHost host);

		/// <summary>Does this DirectiveProcessor support the given directive.</summary>
		/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
		bool IsDirectiveSupported(string directiveName);

		/// <summary>Starts a round of directive processing.</summary>
		/// <param name="templateContents">The contents of the template being processed.</param>
		/// <param name="errors">The collection to report processing errors in.</param>
		void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors);

		/// <summary>Processes a directive from a template file.</summary>
		void ProcessDirective(string directiveName, IDictionary<string, string> arguments);

		/// <summary>Finishes a round of directive processing.</summary>
		void FinishProcessingRun();

		/// <summary>Gets the code to contribute to the generated template processing class because of the most recent run.</summary>
		string GetClassCodeForProcessingRun();

		/// <summary>Gets the code to contribute to the body of the initialize method of the generated template processing class because of the most recent run. This code will run before the base class' Initialize method.</summary>
		string GetPreInitializationCodeForProcessingRun();

		/// <summary>Gets the code to contribute to the body of the initialize method of the generated template processing class because of the most recent run. This code will run after the base class' Initialize method.</summary>
		string GetPostInitializationCodeForProcessingRun();

		/// <summary>Gets any references to pass to the compiler because of the most recent run.</summary>
		string[] GetReferencesForProcessingRun();

		/// <summary>Gets any namespaces to import because of the most recent run.</summary>
		string[] GetImportsForProcessingRun();

		/// <summary>Gets any custom attributes to include on the template class.</summary>
		/// <returns>A collection of custom attributes that can be null or empty.</returns>
		CodeAttributeDeclarationCollection GetTemplateClassCustomAttributes();

		/// <summary>Informs the directive processor whether the run is host-specific.</summary>
		void SetProcessingRunIsHostSpecific(bool hostSpecific);
	}
}
