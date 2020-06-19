using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// HttpWeb request wrapper
	/// </summary>
	public class AsyncHttpWebRequest : IAsyncHttpWebRequest
	{
		private HttpWebRequest request;

		/// <summary>
		/// Gets target Url
		/// </summary>
		public string Url
		{
			get;
		}

		/// <summary>
		/// Gets or sets used method (GET, HEAD, POST, PUT, DELETE, TRACE, or OPTIONS)
		/// </summary>
		public string Method
		{
			get
			{
				return request.Method;
			}
			set
			{
				value.RequiresArgumentNotNullAndNotEmpty("value");
				request.Method = value;
			}
		}

		/// <summary>
		/// Gets or sets timeout for response
		/// </summary>
		public int Timeout
		{
			get
			{
				return request.Timeout;
			}
			set
			{
				request.Timeout = value;
			}
		}

		/// <summary>
		/// Gets or sets cache policy
		/// </summary>
		public RequestCachePolicy CachePolicy
		{
			get
			{
				return request.CachePolicy;
			}
			set
			{
				request.CachePolicy = value;
			}
		}

		/// <summary>
		/// Gets or sets content type. For example, "application/x-www-form-urlencoded"
		/// </summary>
		public string ContentType
		{
			get
			{
				return request.ContentType;
			}
			set
			{
				value.RequiresArgumentNotNullAndNotEmpty("value");
				request.ContentType = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Utilities.Internal.AsyncHttpWebRequest" /> class.
		/// </summary>
		/// <param name="url">Url</param>
		public AsyncHttpWebRequest(string url)
		{
			Url = url;
			request = (WebRequest.Create(url) as HttpWebRequest);
		}

		/// <summary>
		/// Add HTTP headers.
		/// </summary>
		/// <param name="headers">Headers to add</param>
		public void AddHeaders(IEnumerable<KeyValuePair<string, string>> headers)
		{
			headers.RequiresArgumentNotNull("headers");
			foreach (KeyValuePair<string, string> header in headers)
			{
				if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
				{
					request.Headers.Add(header.Key, header.Value);
				}
			}
		}

		/// <summary>
		/// Send request and get response back asynchronously.
		/// </summary>
		/// <returns>Response</returns>
		public async Task<IStreamedHttpWebResponse> GetResponseAsync()
		{
			try
			{
				Task<WebResponse> requestTask = Task.Run(() => request.GetResponse());
				if (await Task.WhenAny(requestTask, Task.Delay(Timeout)).ConfigureAwait(false) == requestTask)
				{
					HttpWebResponse httpWebResponse = (HttpWebResponse)requestTask.Result;
					if (httpWebResponse == null)
					{
						return new StreamedHttpWebResponse
						{
							ErrorCode = ErrorCode.NullResponse
						};
					}
					return new StreamedHttpWebResponse
					{
						ErrorCode = ErrorCode.NoError,
						Response = httpWebResponse
					};
				}
				request.Abort();
				requestTask.SwallowException();
				return new StreamedHttpWebResponse
				{
					ErrorCode = ErrorCode.RequestTimedOut
				};
			}
			catch (WebException ex)
			{
				HttpStatusCode statusCode = HttpStatusCode.Unused;
				if (ex.Status == WebExceptionStatus.ProtocolError)
				{
					statusCode = (ex.Response as HttpWebResponse).StatusCode;
				}
				return new StreamedHttpWebResponse
				{
					ErrorCode = ErrorCode.WebExceptionThrown,
					ExceptionCode = ex.Status,
					StatusCode = statusCode
				};
			}
		}
	}
}
