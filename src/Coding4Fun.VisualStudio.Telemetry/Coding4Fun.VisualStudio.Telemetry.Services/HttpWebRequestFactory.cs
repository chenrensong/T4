using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	[ExcludeFromCodeCoverage]
	internal class HttpWebRequestFactory : IHttpWebRequestFactory
	{
		public IHttpWebRequest Create(string url)
		{
			return new HttpWebRequest(url);
		}

		public IHttpWebRequest Create(string url, IEnumerable<KeyValuePair<string, string>> queryParameters)
		{
			if (queryParameters == null)
			{
				return Create(url);
			}
			UriBuilder uriBuilder = new UriBuilder(url);
			string text = string.Join("&", queryParameters.Select((KeyValuePair<string, string> x) => x.Key + "=" + Uri.EscapeDataString(x.Value)));
			if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
			{
				uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + text;
			}
			else
			{
				uriBuilder.Query = text;
			}
			return new HttpWebRequest(uriBuilder.ToString());
		}
	}
}
