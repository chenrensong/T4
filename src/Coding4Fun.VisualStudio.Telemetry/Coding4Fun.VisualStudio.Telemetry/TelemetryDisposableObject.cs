using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Helper base class to provide virtual method for releasing managed
	/// resources and preventing from calling Dispose several times.
	/// </summary>
	public abstract class TelemetryDisposableObject : IDisposable
	{
		/// <summary>
		/// Gets a value indicating whether session is deposed - to detect redundant calls
		/// </summary>
		public bool IsDisposed
		{
			get;
			private set;
		}

		/// <summary>
		/// This code added to correctly implement the disposable pattern.
		/// </summary>
		public void Dispose()
		{
			if (!IsDisposed)
			{
				DisposeManagedResources();
				IsDisposed = true;
			}
		}

		/// <summary>
		/// This function throws an ObjectDisposedException if the object is disposed.
		/// </summary>
		protected void RequiresNotDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("it is not allowed to use disposed " + GetType() + " object.");
			}
		}

		/// <summary>
		/// User should implement it to dispose managed resources
		/// </summary>
		protected virtual void DisposeManagedResources()
		{
		}
	}
}
