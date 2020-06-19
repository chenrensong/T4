using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Class created to schedule actions either immediately or after a specified delay.
	/// </summary>
	internal interface ITelemetryScheduler
	{
		/// <summary>
		/// Standalone function. Schedules the action to be run by Task.Run.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="token"></param>
		void Schedule(Action action, CancellationToken? token = null);

		/// <summary>
		/// Standalone function. Schedules the delegate to be run by Task.Run.
		/// </summary>
		/// <param name="actionTask"></param>
		/// <param name="token"></param>
		void Schedule(Func<Task> actionTask, CancellationToken? token = null);

		/// <summary>
		/// Initializes the timed feature of this scheduler. The specified
		/// time will be set for every subsequent call of ScheduleTimed.
		/// </summary>
		/// <param name="delay"></param>
		void InitializeTimed(TimeSpan delay);

		/// <summary>
		/// Schedule processing event for the background, using TaskTimer
		/// </summary>
		/// <param name="action"></param>
		void ScheduleTimed(Action action);

		/// <summary>
		/// Schedule processing event for the background, using TaskTimer
		/// </summary>
		/// <param name="actionTask"></param>
		void ScheduleTimed(Func<Task> actionTask);

		/// <summary>
		/// Function that checks whether we should execute timed delegate, in particular,
		/// prevents re-entry if used with a multi-thread lock.
		/// </summary>
		/// <returns></returns>
		bool CanEnterTimedDelegate();

		/// <summary>
		/// Function that should be called after the delegate completes operation.
		/// </summary>
		void ExitTimedDelegate();

		/// <summary>
		/// In case action is scheduled - cancel it
		/// In case no action is scheduled - do nothing
		/// </summary>
		/// <param name="wait"></param>
		void CancelTimed(bool wait = false);
	}
}
