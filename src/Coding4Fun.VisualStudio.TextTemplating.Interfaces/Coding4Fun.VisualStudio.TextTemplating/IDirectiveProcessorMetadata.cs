using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>Metadata provided by DirectiveProcessors. Hosts that want to import DPs via MEF can consume this standard metadata.</summary>
	public interface IDirectiveProcessorMetadata
	{
		/// <summary>Public name of the processor. Used to resolve the processor for a simple host.</summary>
		/// <returns>Returns <see cref="T:System.String" />.</returns>
		string ProcessorName
		{
			get;
		}

		/// <summary>Set of directives supported by the processor. Used by more complex hosts to support anonymous directive processors</summary>
		/// <returns>Returns <see cref="T:System.Collections.Generic.IEnumerable`1" />.</returns>
		IEnumerable<string> SupportedDirectives
		{
			get;
		}
	}
}
