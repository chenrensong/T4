using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal interface IHttpWebRequest
	{
		string Url
		{
			get;
		}

		string Method
		{
			get;
			set;
		}

		RequestCachePolicy CachePolicy
		{
			get;
			set;
		}

		string ContentType
		{
			get;
			set;
		}

		long ContentLength
		{
			get;
			set;
		}

		bool AllowAutoRedirect
		{
			get;
			set;
		}

		void AddHeaders(IEnumerable<KeyValuePair<string, string>> headers);

		Task<IHttpWebResponse> GetResponseAsync(CancellationToken token);

		Task<Stream> GetRequestStreamAsync();
	}
}
