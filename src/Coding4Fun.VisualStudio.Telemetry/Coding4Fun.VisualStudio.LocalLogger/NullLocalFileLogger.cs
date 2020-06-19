using System;

namespace Coding4Fun.VisualStudio.LocalLogger
{
	/// <summary>
	/// Null Local file logger placeholder for non-Windows platforms.
	/// </summary>
	public sealed class NullLocalFileLogger : ILocalFileLogger, IDisposable
	{
		/// <inheritdoc />
		public bool Enabled
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		/// <inheritdoc />
		public string FullLogPath
		{
			get;
		}

		/// <inheritdoc />
		public void Log(LocalLoggerSeverity severity, string componentId, string text)
		{
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
		}
	}
}
