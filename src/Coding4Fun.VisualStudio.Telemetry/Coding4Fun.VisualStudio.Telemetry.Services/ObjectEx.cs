using System;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal static class ObjectEx
	{
		public static bool TryConvertToType<T>(this object originalValue, T defaultValue, out T returnValue)
		{
			if (originalValue == null)
			{
				returnValue = defaultValue;
				return false;
			}
			if (originalValue is T)
			{
				returnValue = (T)originalValue;
			}
			try
			{
				returnValue = (T)Convert.ChangeType(originalValue, typeof(T));
				return true;
			}
			catch
			{
				returnValue = defaultValue;
				return false;
			}
		}
	}
}
