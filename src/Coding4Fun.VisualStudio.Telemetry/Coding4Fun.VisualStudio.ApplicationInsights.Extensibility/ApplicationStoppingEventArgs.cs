using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// Encapsulates arguments of the <see cref="E:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.IApplicationLifecycle.Stopping" /> event.
	/// </summary>
	public class ApplicationStoppingEventArgs : EventArgs
	{
		internal new static readonly ApplicationStoppingEventArgs Empty = new ApplicationStoppingEventArgs((Func<Task> asyncMethod) => asyncMethod());

		private readonly Func<Func<Task>, Task> asyncMethodRunner;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.ApplicationStoppingEventArgs" /> class with the specified runner of asynchronous methods.
		/// </summary>
		public ApplicationStoppingEventArgs(Func<Func<Task>, Task> asyncMethodRunner)
		{
			if (asyncMethodRunner == null)
			{
				throw new ArgumentNullException("asyncMethodRunner");
			}
			this.asyncMethodRunner = asyncMethodRunner;
		}

		/// <summary>
		/// Runs the specified asynchronous method while preventing the application from exiting.
		/// </summary>
		public async void Run(Func<Task> asyncMethod)
		{
			try
			{
				await asyncMethodRunner(asyncMethod);
			}
			catch (Exception ex)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, "Unexpected excption when handling IApplicationLifecycle.Stopping event:{0}", new object[1]
				{
					ex.ToString()
				});
				CoreEventSource.Log.LogError(msg);
			}
		}
	}
}
