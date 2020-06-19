using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Several dictionary extension methods.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Gets a value by the given key.
		/// </summary>
		/// <typeparam name="TK">key type</typeparam>
		/// <typeparam name="TV">value type</typeparam>
		/// <param name="dictionary">dictionary</param>
		/// <param name="key">key in the dictionary</param>
		/// <returns>default if key doesn't exist in the dictionary.</returns>
		public static TV GetOrDefault<TK, TV>(this IDictionary<TK, TV> dictionary, TK key)
		{
			dictionary.TryGetValue(key, out TV value);
			return value;
		}

		/// <summary>
		/// Add one dictionary content to the another dictionary
		/// </summary>
		/// <typeparam name="T">type of the dictionary argument key</typeparam>
		/// <typeparam name="S">type of the dictionary argument value</typeparam>
		/// <param name="target">target dictionary</param>
		/// <param name="source">source dictionary</param>
		/// <param name="forceUpdate">whether we need to force update value</param>
		public static void AddRange<T, S>(this IDictionary<T, S> target, IDictionary<T, S> source, bool forceUpdate = true)
		{
			source.RequiresArgumentNotNull("source");
			foreach (KeyValuePair<T, S> item in source)
			{
				if (forceUpdate || !target.ContainsKey(item.Key))
				{
					target[item.Key] = item.Value;
				}
			}
		}

		/// <summary>
		/// Remove key from the ConcurrentDictionary
		/// </summary>
		/// <typeparam name="TK">type of the dictionary argument key</typeparam>
		/// <typeparam name="TV">type of the dictionary argument value</typeparam>
		/// <param name="dictionary">dictionary</param>
		/// <param name="key">key</param>
		public static void Remove<TK, TV>(this ConcurrentDictionary<TK, TV> dictionary, TK key)
		{
			((IDictionary<TK, TV>)dictionary).Remove(key);
		}
	}
}
