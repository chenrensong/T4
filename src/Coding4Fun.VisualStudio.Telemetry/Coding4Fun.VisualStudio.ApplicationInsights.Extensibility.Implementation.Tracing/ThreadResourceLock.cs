using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	/// <summary>
	/// Thread level resource section lock.
	/// </summary>
	internal class ThreadResourceLock : IDisposable
	{
		/// <summary>
		/// Thread level lock object.
		/// </summary>
		[ThreadStatic]
		private static object syncObject;

		/// <summary>
		/// Gets a value indicating whether lock is set on the section.
		/// </summary>
		public static bool IsResourceLocked => syncObject != null;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing.ThreadResourceLock" /> class.
		/// Marks section locked.
		/// </summary>
		public ThreadResourceLock()
		{
			syncObject = new object();
		}

		/// <summary>
		/// Release lock.
		/// </summary>
		public void Dispose()
		{
			syncObject = null;
		}
	}
}
