using System;
using System.Globalization;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// TypeTools class is a helper to work with types of the C# objects
	/// </summary>
	internal static class TypeTools
	{
		public static bool IsNumericType(Type t)
		{
			if (!(t == typeof(byte)) && !(t == typeof(sbyte)) && !(t == typeof(decimal)) && !(t == typeof(double)) && !(t == typeof(float)) && !(t == typeof(short)) && !(t == typeof(int)) && !(t == typeof(long)) && !(t == typeof(ushort)) && !(t == typeof(uint)))
			{
				return t == typeof(ulong);
			}
			return true;
		}

		/// <summary>
		/// Try convert to UInt. In case of fail return false and init result by default.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryConvertToUInt(object o, out uint result)
		{
			return GuardConvert(Convert.ToUInt32, o, out result);
		}

		/// <summary>
		/// Try convert to Int. In case of fail return false and init result by default.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool TryConvertToInt(object o, out int result)
		{
			return GuardConvert(Convert.ToInt32, o, out result);
		}

		/// <summary>
		/// Convert to String. Never fails.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static string ConvertToString(object o)
		{
			return Convert.ToString(o, CultureInfo.InvariantCulture);
		}

		private static bool GuardConvert<T>(Func<object, T> convertFunc, object from, out T res)
		{
			res = default(T);
			try
			{
				res = convertFunc(from);
				return true;
			}
			catch (Exception ex)
			{
				if (!(ex is FormatException) && !(ex is InvalidCastException) && !(ex is OverflowException))
				{
					throw;
				}
			}
			return false;
		}
	}
}
