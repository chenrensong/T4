using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.LocalLogger
{
	/// <summary>
	/// Controls default logger lifetime
	/// </summary>
	public static class LocalFileLoggerService
	{
		private static readonly object lockDefaultLoggerObject = new object();

		/// <summary>
		/// Gets or sets default logger instance
		/// </summary>
		private static ILocalFileLogger DefaultLogger
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or create and gets default logger instance
		/// </summary>
		public static ILocalFileLogger Default
		{
			get
			{
				if (DefaultLogger == null)
				{
					lock (lockDefaultLoggerObject)
					{
						if (DefaultLogger == null)
						{
							if (Platform.IsWindows)
							{
								DefaultLogger = new LocalFileLogger();
							}
							else
							{
								DefaultLogger = new NullLocalFileLogger();
							}
						}
					}
				}
				return DefaultLogger;
			}
		}
	}
}
