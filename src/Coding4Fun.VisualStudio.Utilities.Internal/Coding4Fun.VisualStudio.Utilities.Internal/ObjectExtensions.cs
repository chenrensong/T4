using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Object extensions methods
	/// </summary>
	public static class ObjectExtensions
	{
		/// <summary>
		/// Returns an enumeration that contains only the given value.
		/// </summary>
		/// <typeparam name="T">type of value</typeparam>
		/// <param name="value">value itself</param>
		/// <returns>INumerable type with only 1 value</returns>
		public static IEnumerable<T> Enumerate<T>(this T value)
		{
			yield return value;
		}

		/// <summary>
		/// Return this enumeration in case it is not null. In case it is null return empty enumeration.
		/// </summary>
		/// <typeparam name="T">type of the values</typeparam>
		/// <param name="enumeration">enumeration</param>
		/// <returns>result</returns>
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumeration)
		{
			return enumeration ?? Enumerable.Empty<T>();
		}
	}
}
