using System;
using System.Collections.Generic;
using System.Reflection;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Class to manage a cache of compiled assemblies in the execution AppDomain.
	/// </summary>
	internal static class AssemblyCache
	{
		/// <summary>
		/// Set of cached assemblies
		/// </summary>
		internal static Dictionary<string, AssemblyRecord> assemblies = new Dictionary<string, AssemblyRecord>(35);

		/// <summary>
		/// Last time cache was used.
		/// </summary>
		internal static DateTime lastUse;

		/// <summary>
		/// Find an assembly containing the code of the given class in the cache
		/// </summary>
		/// <param name="fullClassName"></param>
		/// <returns>An assembly containing the code of the given class or null</returns>
		internal static Assembly Find(string fullClassName)
		{
			lock (assemblies)
			{
				lastUse = DateTime.Now;
				if (assemblies.TryGetValue(fullClassName, out AssemblyRecord value))
				{
					value.LastUse = DateTime.Now;
					return value.Assembly;
				}
				return null;
			}
		}

		internal static void Insert(string fullClassName, Assembly assembly)
		{
			lock (assemblies)
			{
				lastUse = DateTime.Now;
				AssemblyRecord value = new AssemblyRecord(assembly);
				assemblies[fullClassName] = value;
			}
		}
	}
}
