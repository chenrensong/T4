using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal class TaskTimer : IDisposable
	{
		public static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, -1);

		private TimeSpan delay = TimeSpan.FromMinutes(1.0);

		private CancellationTokenSource tokenSource;

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

		public bool IsStarted => tokenSource != null;

		public void Start(Func<Task> elapsed)
		{
			CancellationTokenSource newTokenSource = new CancellationTokenSource();
			Task.Delay(Delay, newTokenSource.Token).ContinueWith((Func<Task, Task>)async delegate
			{
				CancelAndDispose(Interlocked.CompareExchange(ref tokenSource, null, newTokenSource));
				try
				{
					await elapsed();
				}
				catch (Exception ex)
				{
					if (ex is AggregateException)
					{
						foreach (Exception innerException in ((AggregateException)ex).InnerExceptions)
						{
							CoreEventSource.Log.LogError(innerException.ToString());
						}
					}
					CoreEventSource.Log.LogError(ex.ToString());
				}
			}, CancellationToken.None, TaskContinuationOptions.NotOnFaulted | TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			CancelAndDispose(Interlocked.Exchange(ref tokenSource, newTokenSource));
		}

		public void Cancel()
		{
			CancelAndDispose(Interlocked.Exchange(ref tokenSource, null));
		}

		public void Dispose()
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
