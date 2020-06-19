using System;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// CodeContract is used for validate input parameters
	/// </summary>
	public static class CodeContract
	{
		/// <summary>
		/// Requires that argument is not null
		/// </summary>
		/// <typeparam name="T">type of argument</typeparam>
		/// <param name="value">Value</param>
		/// <param name="argumentName">string representation of the argument</param>
		public static void RequiresArgumentNotNull<T>(this T value, string argumentName) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(argumentName);
			}
		}

		/// <summary>
		/// Requires that string is not empty or contains just whitespaces
		/// </summary>
		/// <param name="value">value</param>
		/// <param name="argumentName">string representation of the argument</param>
		public static void RequiresArgumentNotEmptyOrWhitespace(this string value, string argumentName)
		{
			if (value != null && value.IsNullOrWhiteSpace())
			{
				throw new ArgumentException(argumentName + " can't be empty or contains only whitespace characters");
			}
		}

		/// <summary>
		/// Requires that string is not null and not empty
		/// </summary>
		/// <param name="value">value</param>
		/// <param name="argumentName">string representation of the argument</param>
		public static void RequiresArgumentNotNullAndNotEmpty(this string value, string argumentName)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException(argumentName);
			}
		}

		/// <summary>
		/// Requires that argument not null and not whitespace
		/// </summary>
		/// <param name="value">value</param>
		/// <param name="argumentName">string representation of the argument</param>
		public static void RequiresArgumentNotNullAndNotWhiteSpace(this string value, string argumentName)
		{
			if (value.IsNullOrWhiteSpace())
			{
				throw new ArgumentException(argumentName + " can't be null, empty or contains only whitespace characters");
			}
		}

		/// <summary>
		/// Requires that Guid is not empty
		/// </summary>
		/// <param name="guid">Guid to validate</param>
		/// <param name="argumentName">string representation of the argument</param>
		public static void RequiresArgumentNotEmpty(this Guid guid, string argumentName)
		{
			if (guid == Guid.Empty)
			{
				throw new ArgumentException(argumentName + " can't be empty");
			}
		}
	}
}
