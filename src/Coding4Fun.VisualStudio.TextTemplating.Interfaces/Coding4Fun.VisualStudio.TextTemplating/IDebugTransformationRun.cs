using System;
using System.CodeDom.Compiler;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	[CLSCompliant(true)]
	public interface IDebugTransformationRun
	{
		/// <returns>Returns <see cref="T:System.CodeDom.Compiler.CompilerErrorCollection" />.</returns>
		CompilerErrorCollection Errors
		{
			get;
		}

		/// <returns>Returns <see cref="T:System.String" />.</returns>
		string PerformTransformation();
	}
}
