using System;
using System.Composition;

namespace Coding4Fun.VisualStudio.TextTemplating
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	[MetadataAttribute]
	public sealed class SupportedDirectiveAttribute : Attribute
	{
		/// <summary>Contribution to the set of directives supported by the processor. This extra property is required by MEF to build a collection of this name.</summary>
		/// <returns>Returns <see cref="T:System.String" />.</returns>
		public string SupportedDirectives
		{
			get;
			private set;
		}

		/// <summary>Declares that the decorated T4 directive processor supports the given directive.</summary>
		/// <param name="supportedDirective">A directive the processor supports.</param>
		public SupportedDirectiveAttribute(string supportedDirective)
		{
			SupportedDirectives = supportedDirective;
		}
	}
}
