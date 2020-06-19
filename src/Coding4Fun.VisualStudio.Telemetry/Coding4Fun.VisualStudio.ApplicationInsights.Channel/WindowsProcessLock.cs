using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using Coding4Fun.VisualStudio.LocalLogger;
using System;
using System.Globalization;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class WindowsProcessLock : IProcessLock, IDisposable
	{
		private readonly Mutex mutex;

		private readonly string mutexPath;

		public WindowsProcessLock(string name)
		{
			mutexPath = name;
			mutex = new Mutex(false, name);
		}

		public void Acquire(Action action, CancellationToken cancelToken)
		{
			try
			{
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "WindowsProcessLock.Acquire() start for mutex name: {0}", new object[1]
				{
					mutexPath
				}));
				if (mutex.WaitOne() && !cancelToken.IsCancellationRequested && action != null)
				{
					LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "WindowsProcessLock.Acquire() captured mutex, calls action()"));
					action();
				}
			}
			catch (AbandonedMutexException ex)
			{
				CoreEventSource.Log.LogVerbose("Another process/thread abandon the Mutex, try to acquire it and become the active transmitter.");
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", string.Format(CultureInfo.InvariantCulture, "WindowsProcessLock.Acquire() exception happens: {0}", new object[1]
				{
					ex.Message
				}));
				throw new ProcessLockException("Lock was abandoned", ex, true);
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
