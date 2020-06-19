using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Helper class to start Async job with a delay.
	/// Using Start() on already planned task cause cancel current timer and start new one.
	/// Borrowed from Application Insights Coding4Fun.VisualStudio.ApplicationInsights.Implementation.TaskTimer class
	/// </summary>
	internal class TelemetryTaskTimer : TelemetryDisposableObject
	{
		public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

		private TimeSpan delay;

		private CancellationTokenSource tokenSource;

		private Task delayTask;

		private Task currentTask;

		public TimeSpan Delay
		{
			get
			{
				return delay;
			}
			set
			{
				if ((value <= TimeSpan.Zero || value.TotalMilliseconds > 2147483647.0) && value != InfiniteTimeSpan)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				delay = value;
			}
		}

		public bool IsStarted
		{
			get
			{
				if (currentTask != null)
				{
					return !currentTask.IsCompleted;
				}
				return false;
			}
		}

		public TelemetryTaskTimer(TimeSpan taskDelay)
		{
			Delay = taskDelay;
		}

		public void Start(Action elapsed, bool infinite = false)
		{
			CancellationTokenSource newTokenSource = new CancellationTokenSource();
			delayTask = Task.Delay(Delay, newTokenSource.Token);
			currentTask = delayTask.ContinueWith(delegate
			{
				CancelAndDispose(Interlocked.CompareExchange(ref tokenSource, null, newTokenSource));
				if (infinite)
				{
					Start(elapsed, true);
				}
				elapsed();
			}, CancellationToken.None, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			CancelAndDispose(Interlocked.Exchange(ref tokenSource, newTokenSource));
		}

		public void Start(Func<Task> elapsed, bool infinite = false)
		{
			Start(delegate
			{
				elapsed();
			}, infinite);
		}

		public void Cancel()
		{
			CancelAndDispose(Interlocked.Exchange(ref tokenSource, null));
		}

		public void WaitThenCancel()
		{
			if (delayTask != null && currentTask != null && delayTask.IsCompleted)
			{
				currentTask.Wait();
			}
			Cancel();
		}

		protected override void DisposeManagedResources()
		{
			Cancel();
		}

		private static void CancelAndDispose(CancellationTokenSource tokenSource)
		{
			if (tokenSource != null)
			{
				tokenSource.Cancel();
				tokenSource.Dispose();
			}
		}
	}
}
