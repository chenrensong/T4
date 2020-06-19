using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Base class for a concrete DirectiveProcessor
	/// </summary>
	/// <remarks>
	/// A singleton instance of any of these classes that is
	/// required will be held by the Engine.
	/// This class implements a state machine with
	/// the Get... methods only valid after a Start...Finish pair.
	/// </remarks>
	public abstract class DirectiveProcessor : IDirectiveProcessor
	{
		/// <summary>
		/// Error collection for DirectiveProcessor to add Errors/Warnings to.
		/// </summary>
		protected CompilerErrorCollection Errors
		{
			get;
			private set;
		}

		CompilerErrorCollection IDirectiveProcessor.Errors => Errors;

		bool IDirectiveProcessor.RequiresProcessingRunIsHostSpecific => false;

		/// <summary>
		/// Initialize the processor instance
		/// </summary>
		/// <param name="host"></param>
		public virtual void Initialize(ITextTemplatingEngineHost host)
		{
		}

		/// <summary>
		/// Does this DirectiveProcessor support the given directive
		/// </summary>
		/// <remarks>
		/// This call is not connected to the state machine
		/// </remarks>
		/// <param name="directiveName"></param>
		/// <returns></returns>
		public abstract bool IsDirectiveSupported(string directiveName);

		/// <summary>
		/// Begin a round of directive processing
		/// </summary>
		/// <param name="languageProvider"></param>
		/// <param name="templateContents">the contents of the template being processed</param>\
		/// <param name="errors">collection to report processing errors in</param>
		public virtual void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
		{
			Errors = errors;
		}

		/// <summary>
		/// Process a directive from a template file
		/// </summary>
		public abstract void ProcessDirective(string directiveName, IDictionary<string, string> arguments);

		/// <summary>
		/// Finish a round of directive processing
		/// </summary>
		public abstract void FinishProcessingRun();

		/// <summary>
		/// Get the code to contribute to the generated
		/// template processing class as a consequence of the most recent run.
		/// </summary>
		/// <returns>The code that this DirectiveProcessor contributes to the generated TextTemplating class</returns>
		public abstract string GetClassCodeForProcessingRun();

		/// <summary>
		/// Get the code to contribute to the body of the initialize method of the generated
		/// template processing class as a consequence of the most recent run.
		/// This code will run before the base class' Initialize method
		/// </summary>
		/// <returns></returns>
		public abstract string GetPreInitializationCodeForProcessingRun();

		/// <summary>
		/// Get the code to contribute to the body of the initialize method of the generated
		/// template processing class as a consequence of the most recent run.
		/// This code will run after the base class' Initialize method
		/// </summary>
		/// <returns></returns>
		public abstract string GetPostInitializationCodeForProcessingRun();

		/// <summary>
		/// Get any references to pass to the compiler
		/// as a consequence of the most recent run.
		/// </summary>
		/// <returns></returns>
		public abstract string[] GetReferencesForProcessingRun();

		/// <summary>
		/// Get any namespaces to import as a consequence of
		/// the most recent run.
		/// </summary>
		/// <returns></returns>
		public abstract string[] GetImportsForProcessingRun();

		/// <summary>
		/// Get any custom attributes to place on the template class.
		/// </summary>
		/// <returns>A collection of custom attributes that can be null or empty.</returns>
		/// <remarks>
		/// The default implementation is to produce no attributes.
		/// </remarks>
		public virtual CodeAttributeDeclarationCollection GetTemplateClassCustomAttributes()
		{
			return null;
		}

		void IDirectiveProcessor.SetProcessingRunIsHostSpecific(bool hostSpecific)
		{
		}
	}
}
