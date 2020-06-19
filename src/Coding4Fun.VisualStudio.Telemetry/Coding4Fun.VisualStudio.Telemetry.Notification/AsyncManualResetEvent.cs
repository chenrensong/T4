using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.Notification
{
	internal class AsyncManualResetEvent
	{
		private volatile TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

		public Task WaitAsync()
		{
			return tcs.Task;
		}

		public void Set()
		{
			Task.Run(() => tcs.TrySetResult(true));
		}

		public void Reset()
		{
			TaskCompletionSource<bool> taskCompletionSource;
			do
			{
				taskCompletionSource = tcs;
			}
			while (taskCompletionSource.Task.IsCompleted && Interlocked.CompareExchange(ref tcs, new TaskCompletionSource<bool>(), taskCompletionSource) != taskCompletionSource);
		}
	}
}
