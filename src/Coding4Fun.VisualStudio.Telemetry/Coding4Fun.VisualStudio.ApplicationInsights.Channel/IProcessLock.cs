using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal interface IProcessLock : IDisposable
	{
		/// <summary>
		/// Acquires the lock and performs the given action unless cancelled
		/// </summary>
		/// <param name="action">The action to perform when the lock is acquired</param>
		/// <param name="cancelToken">Cancellation token</param>
		void Acquire(Action action, CancellationToken cancelToken);
	}
}
