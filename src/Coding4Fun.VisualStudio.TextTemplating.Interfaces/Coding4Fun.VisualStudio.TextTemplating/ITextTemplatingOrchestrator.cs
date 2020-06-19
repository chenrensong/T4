using System;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	public interface ITextTemplatingOrchestrator
	{
		/// <summary>Event fired when transform for all templates is about to start</summary>
		event EventHandler<TransformingAllTemplatesEventArgs> TransformingAllTemplates;

		/// <summary>Event fired when transform for all templates has completed.</summary>
		event EventHandler<TransformedAllTemplatesEventArgs> TransformedAllTemplates;
	}
}
