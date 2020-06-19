using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Implements an asynchronous transmission of data to an HTTP POST endpoint.
	/// </summary>
	public class Transmission
	{
		private class FipsHMACSHA256 : HMAC
		{
			public FipsHMACSHA256(byte[] key)
			{
				base.HashName = typeof(SHA256CryptoServiceProvider).AssemblyQualifiedName;
				HashSizeValue = 256;
				Key = key;
			}
		}

		internal const string ContentTypeHeader = "Content-Type";

		internal const string ContentEncodingHeader = "Content-Encoding";

		private static readonly string Key = "9418E9E3-1969-413A-8617-A85739D315A1";

		private static readonly HashAlgorithm Encrypter = new FipsHMACSHA256(Encoding.UTF8.GetBytes(Key));

		private static readonly object hashLock = new object();

		private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100.0);

		private int isSending;

		private string contentHash;

		/// <summary>
		/// Gets the Address of the endpoint to which transmission will be sent.
		/// </summary>
		public Uri EndpointAddress
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the content of the transmission.
		/// </summary>
		public byte[] Content
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets content hash, which should be unique per content.
		/// For perf reason it is calculated only when requested for the first time.
		/// </summary>
		public string ContentHash
		{
			get
			{
				if (contentHash == null)
				{
					contentHash = HashContent(Content);
				}
				return contentHash;
			}
		}

		/// <summary>
		/// Gets the content's type of the transmission.
		/// </summary>
		public string ContentType
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the encoding method of the transmission.
		/// </summary>
		public string ContentEncoding
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a timeout value for the transmission.
		/// </summary>
		public TimeSpan Timeout
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.Transmission" /> class.
		/// </summary>
		public Transmission(Uri address, byte[] content, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}
			if (contentType == null)
			{
				throw new ArgumentNullException("contentType");
			}
			EndpointAddress = address;
			Content = content;
			ContentType = contentType;
			ContentEncoding = contentEncoding;
			Timeout = ((timeout == default(TimeSpan)) ? DefaultTimeout : timeout);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.Transmission" /> class. This overload is for Test purposes.
		/// </summary>
		protected internal Transmission()
		{
		}

		/// <summary>
		/// Executes the request that the current transmission represents.
		/// </summary>
		/// <returns>The task to await.</returns>
		public virtual async Task SendAsync(CancellationToken token = default(CancellationToken))
		{
			if (Interlocked.CompareExchange(ref isSending, 1, 0) != 0)
			{
				throw new InvalidOperationException("SendAsync is already in progress.");
			}
			try
			{
				WebRequest request = CreateRequest(EndpointAddress);
				Task timeoutTask = Task.Delay(Timeout);
				Task sendTask = SendRequestAsync(request);
				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
				token.Register(delegate
				{
					tcs.TrySetCanceled();
				}, false);
				Task obj = await Task.WhenAny(timeoutTask, sendTask, tcs.Task).ConfigureAwait(false);
				token.ThrowIfCancellationRequested();
				if (obj == timeoutTask)
				{
					request.Abort();
				}
				await sendTask.ConfigureAwait(false);
			}
			finally
			{
				Interlocked.Exchange(ref isSending, 0);
			}
		}

		/// <summary>
		/// Build string out of object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return "ContentHash: " + ContentHash;
		}

		/// <summary>
		/// Creates a post web request.
		/// </summary>
		/// <param name="address">The Address in the web request.</param>
		/// <returns>A web request pointing to the <c>Address</c>.</returns>
		protected virtual WebRequest CreateRequest(Uri address)
		{
			WebRequest webRequest = WebRequest.Create(address);
			webRequest.Method = "POST";
			if (!string.IsNullOrEmpty(ContentType))
			{
				webRequest.ContentType = ContentType;
			}
			if (!string.IsNullOrEmpty(ContentEncoding))
			{
				webRequest.Headers["Content-Encoding"] = ContentEncoding;
			}
			webRequest.ContentLength = Content.Length;
			return webRequest;
		}

		private async Task SendRequestAsync(WebRequest request)
		{
			using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
			{
				await requestStream.WriteAsync(Content, 0, Content.Length).ConfigureAwait(false);
			}
			using (await request.GetResponseAsync().ConfigureAwait(false))
			{
			}
		}

		private static string HashContent(byte[] content)
		{
			lock (hashLock)
			{
				return BitConverter.ToString(Encrypter.ComputeHash(content)).Replace("-", string.Empty);
			}
		}
	}
}
