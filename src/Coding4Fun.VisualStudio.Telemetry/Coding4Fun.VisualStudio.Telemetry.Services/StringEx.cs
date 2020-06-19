using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal static class StringEx
	{
		private const int MaxPath = 255;

		private static char[] Buffer = new char[255];

		/// <summary>
		/// Normalize path to the Registry friendly. For example, if path is:
		/// " /microsoft/visualstudio//telemetry/experiment/shippedflights " it will be transformed to:
		/// "microsoft\visualstudio\telemetry\experiment\shippedflights".
		/// - trims all whitespace and slash characters from the beginning and end of the string;
		/// - removes multiple slashes occur in the middle;
		/// - convert all '/' slash characters to '\'.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string NormalizePath(this string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}
			int i;
			for (i = 0; i < path.Length && IsSkippable(path[i]); i++)
			{
			}
			if (i == path.Length)
			{
				return string.Empty;
			}
			int num = path.Length - 1;
			while (IsSkippable(path[num]))
			{
				num--;
			}
			int num2 = num - i + 1;
			if (num2 > 255)
			{
				throw new ArgumentException("Too long path", "path");
			}
			char[] array = Interlocked.Exchange(ref Buffer, null);
			if (array == null)
			{
				array = new char[255];
			}
			bool flag = num2 != path.Length;
			int length = 0;
			char c = ' ';
			for (int j = i; j <= num; j++)
			{
				char c2 = path[j];
				if (c2 == '/')
				{
					c2 = '\\';
					flag = true;
				}
				if (c2 == '\\' && c == '\\')
				{
					flag = true;
					continue;
				}
				c = c2;
				array[length++] = c2;
			}
			if (flag)
			{
				path = new string(array, 0, length);
			}
			Interlocked.Exchange(ref Buffer, array);
			return path;
		}

		/// <summary>
		/// Gets the root subcollection of a path. For example, if path is:
		/// microsoft\visualstudio\telemetry, the root subcollection is microsoft
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetRootSubCollectionOfPath(this string path)
		{
			path = path.NormalizePath();
			int num = path.IndexOf('\\');
			if (num > 0)
			{
				return path.Substring(0, num);
			}
			return path;
		}

		/// <summary>
		/// Check whether character is skippable.
		/// </summary>
		/// <param name="c">Input character</param>
		/// <returns>True if character can be skipped</returns>
		private static bool IsSkippable(char c)
		{
			if (!char.IsWhiteSpace(c) && c != '/')
			{
				return c == '\\';
			}
			return true;
		}
	}
}
