using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	[ExcludeFromCodeCoverage]
	internal class HttpWebRequest : IHttpWebRequest
	{
		private System.Net.HttpWebRequest request;

		public string Url
		{
			get;
		}

		public string Method
		{
			get
			{
				return request.Method;
			}
			set
			{
				CodeContract.RequiresArgumentNotNullAndNotEmpty(value, "value");
				request.Method = value;
			}
		}

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

		public string ContentType
		{
			get
			{
				return request.ContentType;
			}
			set
			{
				CodeContract.RequiresArgumentNotNullAndNotEmpty(value, "value");
				request.ContentType = value;
			}
		}

		public long ContentLength
		{
			get
			{
				return request.ContentLength;
			}
			set
			{
				request.ContentLength = value;
			}
		}

		public bool AllowAutoRedirect
		{
			get
			{
				return request.AllowAutoRedirect;
			}
			set
			{
				request.AllowAutoRedirect = value;
			}
		}

		public HttpWebRequest(string url)
		{
			Url = url;
			request = (System.Net.HttpWebRequest)WebRequest.Create(url);
		}

		public void AddHeaders(IEnumerable<KeyValuePair<string, string>> headers)
		{
			CodeContract.RequiresArgumentNotNull<IEnumerable<KeyValuePair<string, string>>>(headers, "headers");
			foreach (KeyValuePair<string, string> header in headers)
			{
				if (!string.IsNullOrEmpty(header.Key) && !string.IsNullOrEmpty(header.Value))
				{
					request.Headers.Add(header.Key, header.Value);
				}
			}
		}

		public async Task<IHttpWebResponse> GetResponseAsync(CancellationToken token)
		{
			try
			{
				using (token.Register(delegate
				{
					request.Abort();
				}, false))
				{
					Task<WebResponse> requestTask = request.GetResponseAsync();
					System.Net.HttpWebResponse httpWebResponse = (System.Net.HttpWebResponse)(await requestTask.ConfigureAwait(false));
					if (token.IsCancellationRequested)
					{
						requestTask.ContinueWith(delegate(Task<WebResponse> task)
						{
							_ = task.Exception;
						});
						return new HttpWebResponse
						{
							ErrorCode = ErrorCode.RequestTimedOut
						};
					}
					if (httpWebResponse == null)
					{
						return new HttpWebResponse
						{
							ErrorCode = ErrorCode.NullResponse
						};
					}
					return new HttpWebResponse
					{
						ErrorCode = ErrorCode.NoError,
						Response = httpWebResponse,
						Headers = httpWebResponse.Headers
					};
				}
			}
			catch (Exception ex)
			{
				WebException ex2 = (!(ex is AggregateException)) ? (ex as WebException) : (ex.InnerException as WebException);
				if (ex2 == null)
				{
					throw;
				}
				HttpStatusCode statusCode = HttpStatusCode.Unused;
				if (ex2.Status == WebExceptionStatus.ProtocolError)
				{
					statusCode = (ex2.Response as System.Net.HttpWebResponse).StatusCode;
				}
				return new HttpWebResponse
				{
					ErrorCode = ErrorCode.WebExceptionThrown,
					ExceptionCode = ex2.Status,
					StatusCode = statusCode
				};
			}
		}

		public Task<Stream> GetRequestStreamAsync()
		{
			return request.GetRequestStreamAsync();
		}
	}
}
