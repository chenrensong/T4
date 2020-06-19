using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Task extensions
	/// </summary>
	public static class TaskExtensions
	{
		/// <summary>
		/// Swallow exceptions for event handlers.
		/// http://theburningmonk.com/2012/10/c-beware-of-async-void-in-your-code/
		///
		/// We need to read an Exception to prevent throwing an Exception for
		/// .NET 4.0 and below. See
		/// http://stackoverflow.com/questions/25691114/where-does-an-async-task-throw-exception-if-it-is-not-awaited
		/// </summary>
		/// <param name="task">Task to swallow exception for</param>
		public static void SwallowException(this Task task)
		{
			task.ContinueWith(delegate(Task t)
			{
				_ = t.Exception;
			});
		}
	}
}
