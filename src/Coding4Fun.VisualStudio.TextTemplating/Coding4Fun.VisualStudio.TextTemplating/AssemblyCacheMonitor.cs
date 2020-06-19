using System;
using System.Linq;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Class to monitor the state of the assembly cache
	/// </summary>
	public sealed class AssemblyCacheMonitor : MarshalByRefObject
	{
		/// <summary>
		/// Returns how many assemblies the cache thinks are stale
		/// </summary>
		public int GetStaleAssembliesCount(TimeSpan assemblyStaleTime)
		{
			int num = 0;
			lock (AssemblyCache.assemblies)
			{
				return AssemblyCache.assemblies.Keys.Count((string key) => AssemblyCache.lastUse - AssemblyCache.assemblies[key].LastUse > assemblyStaleTime);
			}
		}
	}
}
