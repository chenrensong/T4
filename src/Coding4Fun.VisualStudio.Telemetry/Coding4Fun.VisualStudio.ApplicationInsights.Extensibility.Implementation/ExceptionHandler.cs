using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal static class ExceptionHandler
	{
		/// <summary>
		/// Starts the <paramref name="asyncMethod" />, catches and logs any exceptions it may throw.
		/// </summary>
		public static void Start(Func<Task> asyncMethod)
		{
			try
			{
				asyncMethod().ContinueWith(delegate(Task task)
				{
					CoreEventSource.Log.LogError(task.Exception.ToString());
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
			catch (Exception ex)
			{
				CoreEventSource.Log.LogError(ex.ToString());
			}
		}
	}
}
