using System;
using System.Composition;

namespace Coding4Fun.VisualStudio.TextTemplating
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	[MetadataAttribute]
	public sealed class DirectiveProcessorAttribute : ExportAttribute
	{
		/// <summary>The public name of the processor.</summary>
		/// <returns>Returns <see cref="T:System.String" />.</returns>
		public string ProcessorName
		{
			get;
			private set;
		}

		/// <summary>Declares the decorated type to be a T4 directive processor</summary>
		/// <param name="processorName">The public name of the processor.</param>
		public DirectiveProcessorAttribute(string processorName)
			: base(typeof(IDirectiveProcessor))
		{
			ProcessorName = processorName;
		}
	}
}
