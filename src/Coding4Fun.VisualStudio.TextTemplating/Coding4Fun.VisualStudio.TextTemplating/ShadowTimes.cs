using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Helper class to manage a record of the last write dates of reference assemblies that are shadow-copied in the templating appdomain.
	/// </summary>
	internal static class ShadowTimes
	{
		/// <summary>
		/// Set of shadow-copying times by assembly's full location
		/// </summary>
		/// <remarks>Typically unlikely that the dictionary will be accessed on multiple threads so use concurrency level of one.</remarks>
		private static readonly ConcurrentDictionary<string, DateTime> times = new ConcurrentDictionary<string, DateTime>(1, 10, StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Are any of the shadow-copied assemblies obsolete with respect to their on-disk files.
		/// </summary>
		/// <returns>Whether any assemblies are obsolete</returns>
		internal static bool AreShadowCopiesObsolete
		{
			get
			{
				if (AppDomain.CurrentDomain.ShadowCopyFiles)
				{
					IEnumerable<string> enumerable = times.Keys.ToList();
					foreach (string item in enumerable)
					{
						if (File.Exists(item))
						{
							FileInfo fileInfo = new FileInfo(item);
							if (IsAssemblyObsolete(item, fileInfo.LastWriteTime))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Record the modification time of an assembly to be shadow-copied
		/// </summary>
		internal static void Insert(string assemblyLocation, DateTime time)
		{
			times.TryAdd(assemblyLocation, time);
		}

		private static bool IsAssemblyObsolete(string assemblyLocation, DateTime lastWriteTime)
		{
			if (times.TryGetValue(assemblyLocation, out DateTime value))
			{
				return value < lastWriteTime;
			}
			return false;
		}
	}
}
