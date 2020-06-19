using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Class to resolve assembly references in the T4 execution AppDomain
	/// </summary>
	/// <remarks>
	/// This is modeled as a separate class purely because the event seems to need its implemented class to be marked Serializable.
	/// </remarks>
	[Serializable]
	internal class AssemblyResolver
	{
		/// <summary>
		/// Resolve assembly references in the T4 execution app domain
		/// </summary>
		public Assembly AssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName) 
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			return assemblies.FirstOrDefault((Assembly assembly) => StringComparer.OrdinalIgnoreCase.Compare(assembly.FullName, assemblyName.Name) == 0);
		}
	}
}
