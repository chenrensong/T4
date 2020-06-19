using System.IO;
using System.Net;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// HttpWeb response wrapper with stream implementation.
	/// </summary>
	public class StreamedHttpWebResponse : IStreamedHttpWebResponse
	{
		/// <summary>
		/// Gets or sets response error code.
		/// </summary>
		public ErrorCode ErrorCode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets response exception code.
		/// </summary>
		public WebExceptionStatus ExceptionCode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets response itself.
		/// </summary>
		public HttpWebResponse Response
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets response status code.
		/// </summary>
		public HttpStatusCode StatusCode
		{
			get;
			set;
		}

		/// <summary>
		/// Get response as a stream.
		/// </summary>
		/// <returns>Response stream</returns>
		public Stream GetResponseStream()
		{
			Stream result = null;
			if (ErrorCode == ErrorCode.NoError)
			{
				result = Response.GetResponseStream();
			}
			return result;
		}
	}
}
