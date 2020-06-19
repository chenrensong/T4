using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// A SynchronizationContext whose synchronously blocking Wait method does not allow
	/// any reentrancy via the message pump.
	/// </summary>
	internal class NoPumpSyncContext : SynchronizationContext
	{
		/// <summary>
		/// A shared singleton.
		/// </summary>
		private static readonly SynchronizationContext DefaultInstance = new NoPumpSyncContext();

		/// <summary>
		/// Gets a shared instance of this class.
		/// </summary>
		public static SynchronizationContext Default => DefaultInstance;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting.NoPumpSyncContext" /> class.
		/// </summary>
		public NoPumpSyncContext()
		{
			SetWaitNotificationRequired();
		}

		/// <summary>
		/// Synchronously blocks without a message pump.
		/// </summary>
		/// <param name="waitHandles">An array of type <see cref="T:System.IntPtr" /> that contains the native operating system handles.</param>
		/// <param name="waitAll">true to wait for all handles; false to wait for any handle.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite" /> (-1) to wait indefinitely.</param>
		/// <returns>
		/// The array index of the object that satisfied the wait.
		/// </returns>
		public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			return NativeMethods.WaitForMultipleObjects((uint)waitHandles.Length, waitHandles, waitAll, (uint)millisecondsTimeout);
		}
	}
}
