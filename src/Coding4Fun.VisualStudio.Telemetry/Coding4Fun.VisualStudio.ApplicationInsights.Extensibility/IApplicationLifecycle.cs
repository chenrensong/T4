using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// Encapsulates application lifecycle events.
	/// </summary>
	public interface IApplicationLifecycle
	{
		/// <summary>
		/// Occurs when a new instance of the application is started or an existing instance is activated.
		/// </summary>
		event Action<object, object> Started;

		/// <summary>
		/// Occurs when the application is suspending or closing.
		/// </summary>
		event EventHandler<ApplicationStoppingEventArgs> Stopping;
	}
}
