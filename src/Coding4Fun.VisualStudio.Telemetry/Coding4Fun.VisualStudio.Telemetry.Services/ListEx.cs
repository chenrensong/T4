using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal static class ListEx
	{
		public static void AddIfNotEmpty(this List<string> parts, string v)
		{
			if (!string.IsNullOrEmpty(v))
			{
				parts.Add(v);
			}
		}
	}
}
