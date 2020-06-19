using System.IO;
using System.Net;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// HttpWeb response wrapper interface.
	/// </summary>
	public interface IStreamedHttpWebResponse
	{
		/// <summary>
		/// Gets response error code.
		/// </summary>
		ErrorCode ErrorCode
		{
			get;
		}

		/// <summary>
		/// Gets response exception code.
		/// </summary>
		WebExceptionStatus ExceptionCode
		{
			get;
		}

		/// <summary>
		/// Gets response status code.
		/// </summary>
		HttpStatusCode StatusCode
		{
			get;
		}

		/// <summary>
		/// Get response as a stream.
		/// </summary>
		/// <returns>Response stream</returns>
		Stream GetResponseStream();
	}
}
