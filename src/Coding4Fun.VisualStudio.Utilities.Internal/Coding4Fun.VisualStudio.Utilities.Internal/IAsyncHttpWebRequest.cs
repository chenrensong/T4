using System.Collections.Generic;
using System.Net.Cache;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// HttpWeb request wrapper interface.
	/// </summary>
	public interface IAsyncHttpWebRequest
	{
		/// <summary>
		/// Gets target Url
		/// </summary>
		string Url
		{
			get;
		}

		/// <summary>
		/// Gets or sets used method (GET, HEAD, POST, PUT, DELETE, TRACE, or OPTIONS)
		/// </summary>
		string Method
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets timeout for response
		/// </summary>
		int Timeout
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets cache policy
		/// </summary>
		RequestCachePolicy CachePolicy
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets content type. For example, "application/x-www-form-urlencoded"
		/// </summary>
		string ContentType
		{
			get;
			set;
		}

		/// <summary>
		/// Add HTTP headers.
		/// </summary>
		/// <param name="headers">Headers</param>
		void AddHeaders(IEnumerable<KeyValuePair<string, string>> headers);

		/// <summary>
		/// Send request and get response back asynchronously.
		/// </summary>
		/// <returns>Response</returns>
		Task<IStreamedHttpWebResponse> GetResponseAsync();
	}
}
