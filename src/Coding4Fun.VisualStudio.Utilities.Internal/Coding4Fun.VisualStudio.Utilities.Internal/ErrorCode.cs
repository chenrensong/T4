namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Possible error codes for response.
	/// </summary>
	public enum ErrorCode
	{
		/// <summary>
		/// No error occurs
		/// </summary>
		NoError,
		/// <summary>
		/// Null response was returned.
		/// </summary>
		NullResponse,
		/// <summary>
		/// Request was cancelled by timeout.
		/// </summary>
		RequestTimedOut,
		/// <summary>
		/// Other web exception was thrown.
		/// </summary>
		WebExceptionThrown
	}
}
