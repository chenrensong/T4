using System;
using System.Globalization;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	/// <summary>
	/// Provides a set of extension methods for tracing.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Returns a culture-independent string representation of the given <paramref name="exception" /> object,
		/// appropriate for diagnostics tracing.
		/// </summary>
		/// <returns></returns>
		public static string ToInvariantString(this Exception exception)
		{
			CultureInfo currentUICulture = Thread.CurrentThread.CurrentUICulture;
			try
			{
				Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
				return exception.ToString();
			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = currentUICulture;
			}
		}
	}
}
