using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	internal static class SpinWait
	{
		internal static void ExecuteSpinWaitLock(this object syncRoot, Action action)
		{
			while (!Monitor.TryEnter(syncRoot, 0))
			{
			}
			try
			{
				action();
			}
			finally
			{
				Monitor.Exit(syncRoot);
			}
		}
	}
}
