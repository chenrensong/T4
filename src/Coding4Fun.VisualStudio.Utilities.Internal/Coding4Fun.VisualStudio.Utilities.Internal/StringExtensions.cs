using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Extend several strings methods
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Join string together using "separator" string as separator
		/// </summary>
		/// <param name="values">string array</param>
		/// <param name="separator">separator string </param>
		/// <returns>new string</returns>
		public static string Join(this IEnumerable<string> values, string separator)
		{
			return string.Join(separator, values);
		}

		/// <summary>
		/// Check whether string is null or contains whitespaces only
		/// </summary>
		/// <param name="value">string to validate</param>
		/// <returns>result of operation</returns>
		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}
	}
}
