namespace Coding4Fun.VisualStudio.LocalLogger
{
	/// <summary>
	/// Logger interface
	/// </summary>
	public interface ILocalFileLogger
	{
		/// <summary>
		/// Gets or sets a value indicating whether logger enabled or not.
		/// </summary>
		bool Enabled
		{
			get;
			set;
		}

		/// <summary>
		/// Gets full logger file path
		/// </summary>
		string FullLogPath
		{
			get;
		}

		/// <summary>
		/// Log current string with severity
		/// </summary>
		/// <param name="severity">Severity</param>
		/// <param name="componentId">Component id, so it would be easy to filter later, might be null or empty</param>
		/// <param name="text">Required free form logger text</param>
		void Log(LocalLoggerSeverity severity, string componentId, string text);
	}
}
