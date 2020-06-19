using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	[ExcludeFromCodeCoverage]
	internal class HttpWebResponse : IHttpWebResponse
	{
		public ErrorCode ErrorCode
		{
			get;
			set;
		}

		public WebExceptionStatus ExceptionCode
		{
			get;
			set;
		}

		public System.Net.HttpWebResponse Response
		{
			get;
			set;
		}

		public HttpStatusCode StatusCode
		{
			get;
			set;
		}

		public WebHeaderCollection Headers
		{
			get;
			set;
		}

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
