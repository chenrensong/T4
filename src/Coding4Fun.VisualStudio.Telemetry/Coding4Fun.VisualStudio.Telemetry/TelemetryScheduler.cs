using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryScheduler : ITelemetryScheduler
	{
		internal const int IsInProgressOfProcessingEvent = 1;

		internal const int NotInProgressOfProcessingEvent = 0;

		private int isInProcess;

		private TelemetryTaskTimer taskTimer;

		internal int IsInProgress => isInProcess;

		/// <summary>
		/// Standalone function. Schedules the action to be run by Task.Run.
		/// </summary>
		/// <param name="actionTask"></param>
		/// <param name="token"></param>
		public void Schedule(Func<Task> actionTask, CancellationToken? token = null)
		{
			CodeContract.RequiresArgumentNotNull<Func<Task>>(actionTask, "actionTask");
			if (token.HasValue)
			{
				Task.Run(actionTask, token.Value);
			}
			else
			{
				Task.Run(actionTask);
			}
		}

		/// <summary>
		/// Standalone function. Schedules the delegate to be run by Task.Run.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="token"></param>
		public void Schedule(Action action, CancellationToken? token = null)
		{
			CodeContract.RequiresArgumentNotNull<Action>(action, "action");
			if (token.HasValue)
			{
				Task.Run(action, token.Value);
			}
			else
			{
				Task.Run(action);
			}
		}

		/// <summary>
		/// Initializes the timed feature of this scheduler. The specified
		/// time will be set for every subsequent call of ScheduleTimed.
		/// </summary>
		/// <param name="delay"></param>
		public void InitializeTimed(TimeSpan delay)
		{
			if (taskTimer != null)
			{
				throw new ArgumentException("cannot initialize twice");
			}
			taskTimer = new TelemetryTaskTimer(delay);
		}

		/// <summary>
		/// Schedule processing event for the background, using TaskTimer
		/// </summary>
		/// <param name="actionTask"></param>
		public void ScheduleTimed(Func<Task> actionTask)
		{
			if (taskTimer == null)
			{
				throw new InvalidOperationException("Cannot mix usage of scheduler");
			}
			CodeContract.RequiresArgumentNotNull<Func<Task>>(actionTask, "actionTask");
			if (!taskTimer.IsStarted)
			{
				taskTimer.Start(actionTask, true);
			}
		}

		/// <summary>
		/// Schedule processing event for the background, using TaskTimer
		/// </summary>
		/// <param name="action"></param>
		public void ScheduleTimed(Action action)
		{
			if (taskTimer == null)
			{
				throw new InvalidOperationException("Cannot mix usage of scheduler");
			}
			CodeContract.RequiresArgumentNotNull<Action>(action, "action");
			if (!taskTimer.IsStarted)
			{
				taskTimer.Start(action);
			}
		}

		/// <summary>
		/// Function that checks whether we should execute timed delegate, in particular,
		/// prevents re-entry if used with a multi-thread lock.
		/// </summary>
		/// <returns></returns>
		public bool CanEnterTimedDelegate()
		{
			if (taskTimer == null)
			{
				throw new InvalidOperationException("Cannot mix usage of scheduler");
			}
			return Interlocked.CompareExchange(ref isInProcess, 1, 0) != 1;
		}

		/// <summary>
		/// Function that should be called after the delegate completes operation.
		/// </summary>
		public void ExitTimedDelegate()
		{
			if (taskTimer == null)
			{
				throw new InvalidOperationException("Cannot mix usage of scheduler");
			}
			if (isInProcess == 0)
			{
				throw new InvalidOperationException("Cannot exit before enter");
			}
			isInProcess = 0;
		}

		/// <summary>
		/// In case action is scheduled - cancel it
		/// In case no action is scheduled - do nothing
		/// </summary>
		/// <param name="wait"></param>
		public void CancelTimed(bool wait = false)
		{
			if (taskTimer == null)
			{
				throw new InvalidOperationException("Cannot mix usage of scheduler");
			}
			if (wait)
			{
				taskTimer.WaitThenCancel();
			}
			else
			{
				taskTimer.Cancel();
			}
		}
	}
}
