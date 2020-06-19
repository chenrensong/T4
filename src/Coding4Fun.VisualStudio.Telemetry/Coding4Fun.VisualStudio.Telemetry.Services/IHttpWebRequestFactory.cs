using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal interface IHttpWebRequestFactory
	{
		IHttpWebRequest Create(string url);

		IHttpWebRequest Create(string url, IEnumerable<KeyValuePair<string, string>> queryParameters);
	}
}
