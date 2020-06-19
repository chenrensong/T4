using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Coding4Fun.VisualStudio.LocalLogger
{
	/// <summary>
	/// Local file logger writes one line per event.
	/// File path specified once per creation and can't be changed during lifetime.
	/// It is possible to Enable/Disable logger during the object lifetime. By default logger is created disabled.
	/// Once logger enabled log file is either created or opened for appending.
	/// Flush on disk happens after every record to be sure that data will persist even in the case of abnormal program termination.
	/// Use soft dispose method. No exceptions after dispose, just no-op. That would help for centralized logger, when logger is disposed and some
	/// stale component trying to use it. We don't want to fail the whole application in this case.
	/// </summary>
	public sealed class LocalFileLogger : ILocalFileLogger, IDisposable
	{
		private const string UnknownName = "unknown";

		private static int sequenceNumber;

		private readonly ITextWriterFactory textWriterFactory;

		/// <summary>
		/// The number of times this object was disposed.
		/// </summary>
		private int disposeCount;

		private TextWriter writer;

		private bool isEnabled;

		/// <inheritdoc />
		public bool Enabled
		{
			get
			{
				return isEnabled;
			}
			set
			{
				if (disposeCount <= 0 && isEnabled != value)
				{
					if (value)
					{
						CreateOrOpenFile();
					}
					else
					{
						CloseFile();
					}
					isEnabled = value;
				}
			}
		}

		/// <inheritdoc />
		public string FullLogPath
		{
			get;
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public LocalFileLogger()
			: this(GenerateLogFileName())
		{
		}

		/// <summary>
		/// Constructor with specified logger file name
		/// </summary>
		/// <param name="logFilePath"></param>
		public LocalFileLogger(string logFilePath)
			: this(new DefaultTextWriterFactory(), logFilePath)
		{
		}

		/// <summary>
		/// Internal constructor for unit testing
		/// </summary>
		/// <param name="textWriterFactory"></param>
		/// <param name="logFilePath"></param>
		internal LocalFileLogger(ITextWriterFactory textWriterFactory, string logFilePath)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(logFilePath, "logFilePath");
			CodeContract.RequiresArgumentNotNull<ITextWriterFactory>(textWriterFactory, "textWriterFactory");
			FullLogPath = logFilePath;
			this.textWriterFactory = textWriterFactory;
		}

		/// <inheritdoc />
		public void Log(LocalLoggerSeverity severity, string componentId, string text)
		{
			if (disposeCount <= 0 && writer != null && !string.IsNullOrEmpty(text))
			{
				if (string.IsNullOrEmpty(componentId))
				{
					componentId = "unknown";
				}
				try
				{
					writer?.WriteLineAsync(string.Format(CultureInfo.InvariantCulture, "[{0:yyyy-MM-dd HH:mm:ss.fff}]\t{1}\t{2}\t{3}\t{4}", DateTime.Now, NativeMethods.GetCurrentThreadId(), severity.ToString(), componentId, text));
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Generate unique file name. Unique name guaranteed by process name, process id, datatime, incremental sequence number per process.
		/// </summary>
		/// <returns>Generated full path name</returns>
		internal static string GenerateLogFileName()
		{
			string tempPath = Path.GetTempPath();
			string path = NativeMethods.GetFullProcessExeName() ?? "unknown";
			path = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
			int num = Interlocked.Increment(ref sequenceNumber);
			string path2 = string.Format(CultureInfo.InvariantCulture, "vstel_{0:yyyyMMddHHmmss}_{1}_{2}_{3}.log", DateTime.Now, path, NativeMethods.GetCurrentProcessId(), num);
			return Path.Combine(tempPath, path2);
		}

		private void Dispose(bool disposing)
		{
			if (Interlocked.Increment(ref disposeCount) == 1)
			{
				CloseFile();
			}
		}

		private void CreateOrOpenFile()
		{
			try
			{
				writer = textWriterFactory.CreateTextWriter(FullLogPath);
			}
			catch
			{
			}
		}

		private void CloseFile()
		{
			writer?.Dispose();
			writer = null;
		}
	}
}
