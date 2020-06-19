namespace Coding4Fun.VisualStudio.LocalLogger
{
	/// <summary>
	/// Severity of the message
	/// </summary>
	public enum LocalLoggerSeverity
	{
		/// <summary>
		/// Just informational message.
		/// </summary>
		Info,
		/// <summary>
		/// Warning.
		/// </summary>
		Warning,
		/// <summary>
		/// Error.
		/// </summary>
		Error,
		/// <summary>
		/// Critical error, can't continue.
		/// </summary>
		Critical
	}
}
