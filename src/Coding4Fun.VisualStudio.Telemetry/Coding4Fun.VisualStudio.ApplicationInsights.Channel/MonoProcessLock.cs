using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class MonoProcessLock : IProcessLock, IDisposable
	{
		private readonly FileBasedMutex mutex;

		public MonoProcessLock(string name)
		{
			mutex = new FileBasedMutex(name);
		}

		public void Acquire(Action action, CancellationToken cancelToken)
		{
			try
			{
				if (!mutex.AcquireMutex(cancelToken))
				{
					throw new ProcessLockException("Cannot acquire file-based mutex");
				}
				if (!cancelToken.IsCancellationRequested)
				{
					action?.Invoke();
				}
			}
			catch (Exception innerException)
			{
				throw new ProcessLockException("Exception while acquiring file-based mutext", innerException);
			}
		}

		public void Dispose()
		{
			if (mutex != null)
			{
				mutex.Dispose();
			}
		}
	}
}
