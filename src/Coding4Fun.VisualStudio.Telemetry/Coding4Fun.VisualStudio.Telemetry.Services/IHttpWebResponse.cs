using System.IO;
using System.Net;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal interface IHttpWebResponse
	{
		ErrorCode ErrorCode
		{
			get;
		}

		WebExceptionStatus ExceptionCode
		{
			get;
		}

		HttpStatusCode StatusCode
		{
			get;
		}

		WebHeaderCollection Headers
		{
			get;
			set;
		}

		Stream GetResponseStream();
	}
}
