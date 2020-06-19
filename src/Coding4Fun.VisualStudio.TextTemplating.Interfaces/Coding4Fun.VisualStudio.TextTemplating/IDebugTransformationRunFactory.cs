using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	[CLSCompliant(true)]
	public interface IDebugTransformationRunFactory
	{
		/// <returns>Returns <see cref="T:Coding4Fun.VisualStudio.TextTemplating.IDebugTransformationRun" />.</returns>
		IDebugTransformationRun CreateTransformationRun(Type t, string content, Func<AssemblyLoadContext, AssemblyName, Assembly?>? resolver);
	}
}
