using System;
using System.Collections.Generic;
using System.Reflection;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Defines extension methods that allow coding against <see cref="T:System.Type" /> without conditional compilation on versions of .NET framework.
	/// </summary>
	internal static class TypeExtensions
	{
		public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type)
		{
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static bool IsAbstract(this Type type)
		{
			return type.IsAbstract;
		}

		public static bool IsGenericType(this Type type)
		{
			return type.IsGenericType;
		}
	}
}
