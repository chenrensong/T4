using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Process lock exception indicates an error that happened within the ProcessLock.
	/// </summary>
	internal class ProcessLockException : Exception
	{
		/// <summary>
		/// Gets a value indicating whether consumer should retry acquiring the process lock or just give up.
		/// </summary>
		public bool IsRetryable
		{
			get;
		}

		public ProcessLockException(string description, Exception innerException = null, bool isRetryable = false)
			: base(description, innerException)
		{
			IsRetryable = isRetryable;
		}
	}
}
