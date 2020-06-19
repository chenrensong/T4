using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// Provides operation to issue HTTP requests to obtain a file, either from the server or from the local IE cache.
	/// </summary>
	internal sealed class RemoteControlHTTPRequestor : IRemoteControlHTTPRequestor
	{
		private const string ErrorMarkerFileExtension = ".errormarker";

		private const string AgeHeaderName = "Age";

		private const string CancelledMessage = "Download was cancelled by caller";

		private const string WebExceptionMessage = "Reading HTTP response stream throws an WebException";

		private const int MinRetryIntervalSeconds = 2;

		private const int MaxRetryIntervalSeconds = 32;

		private const int ConvertMilliToSeconds = 1000;

		private static readonly string TempFileDir = Path.Combine(Path.GetTempPath(), "VSRemoteControl");

		private static readonly Semaphore TempFileCleanupSem = new Semaphore(1, 1);

		private static readonly HttpRequestCachePolicy CacheOnlyPolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheOnly);

		private static readonly HttpRequestCachePolicy ServerRevalidatePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Revalidate);

		private readonly string url;

		private readonly string errorMarkerFileUrl;

		private readonly int httpRequestTimeoutMillis;

		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="url">Server URL to obtain the settings file</param>
		/// <param name="httpRequestTimeoutMillis">Timeout in milliseconds for the HTTP requests issued by this
		/// class</param>
		internal RemoteControlHTTPRequestor(string url, int httpRequestTimeoutMillis)
			: this()
		{
			url = url.TrimEnd('/');
			if (url.EndsWith(".errormarker", StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "url '{0}' is not allowed. url argument must not end with {1}", new object[2]
				{
					url,
					".errormarker"
				}));
			}
			this.url = url;
			errorMarkerFileUrl = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[2]
			{
				url,
				".errormarker"
			});
			this.httpRequestTimeoutMillis = httpRequestTimeoutMillis;
		}

		private RemoteControlHTTPRequestor()
		{
		}

		/// <summary>
		/// Reads the file from the server url.
		/// In case of errors reading the file from the server, returned <see cref="T:Coding4Fun.VisualStudio.RemoteControl.GetFileResult" /> object's
		/// IsSuccessStatusCode value will be false.
		/// </summary>
		/// <returns>Information about the file obtained from the server</returns>
		async Task<GetFileResult> IRemoteControlHTTPRequestor.GetFileFromServerAsync()
		{
			GetFileResult result = new GetFileResult
			{
				Code = HttpStatusCode.Unused
			};
			try
			{
				int i = 0;
				while (true)
				{
					if (i >= 2)
					{
						return result;
					}
					if (i > 0)
					{
						int num = Math.Min(32, (int)Math.Pow(2.0, i));
						try
						{
							await Task.Delay(num * 1000, cancellationTokenSource.Token).ConfigureAwait(false);
						}
						catch (OperationCanceledException)
						{
							return result;
						}
						catch (ObjectDisposedException)
						{
							return result;
						}
						result.Dispose();
					}
					result = await GetFile(url, httpRequestTimeoutMillis, ServerRevalidatePolicy).ConfigureAwait(false);
					if (result.IsSuccessStatusCode)
					{
						break;
					}
					i++;
				}
				return result;
			}
			finally
			{
				if (!result.IsSuccessStatusCode && Platform.IsWindows)
				{
					WinINetHelper.WriteErrorResponseToCache(errorMarkerFileUrl, result.Code);
				}
			}
		}

		/// <summary>
		/// Reads the file from the local IE cache only.
		/// If the file does not exist in the cache, the returned <see cref="T:Coding4Fun.VisualStudio.RemoteControl.GetFileResult" /> object's IsFromCache value
		/// will be false and Code will be Unused.
		/// </summary>
		/// <returns>Information about the file in the IE cache</returns>
		async Task<GetFileResult> IRemoteControlHTTPRequestor.GetFileFromCacheAsync()
		{
			return await GetFile(url, httpRequestTimeoutMillis, CacheOnlyPolicy).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets the elapsed time (in seconds) since the last error in downloading / revalidating the file from the
		/// server.
		/// </summary>
		/// <returns>Time in seconds since last error or Int.MaxValue if no error has ever occured.</returns>
		async Task<int> IRemoteControlHTTPRequestor.LastServerRequestErrorSecondsAgoAsync()
		{
			using (GetFileResult getFileResult = await GetFile(errorMarkerFileUrl, httpRequestTimeoutMillis, CacheOnlyPolicy).ConfigureAwait(false))
			{
				if (getFileResult.IsFromCache)
				{
					return getFileResult.AgeSeconds.Value;
				}
				return int.MaxValue;
			}
		}

		/// <summary>
		/// Cancels all in progress HTTP requests. Any future calls to this class should not be made.
		/// </summary>
		void IRemoteControlHTTPRequestor.Cancel()
		{
			cancellationTokenSource.Cancel();
		}

		/// <summary>
		/// Extracts the value of Age header from <paramref name="resp" />.
		/// </summary>
		/// <param name="resp">HTTP response</param>
		/// <returns>If Age header is present on <paramref name="resp" />can is valid,
		/// returns its value. Otherwise null.</returns>
		private static int? ExtractAgeHeaderValue(HttpWebResponse resp)
		{
			string[] values = resp.Headers.GetValues("Age");
			if (values != null && values.Length != 0 && !string.IsNullOrEmpty(values[0]))
			{
				int result = 0;
				if (int.TryParse(values[0], out result))
				{
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// Starts a maintanence task that tries to delete old temp files left behind.
		/// </summary>
		private static void StartTempFileCleanup()
		{
			Task.Run(delegate
			{
				if (TempFileCleanupSem.WaitOne(0, false))
				{
					try
					{
						string[] files;
						try
						{
							files = Directory.GetFiles(TempFileDir, "*", SearchOption.AllDirectories);
						}
						catch (IOException)
						{
							return;
						}
						catch (UnauthorizedAccessException)
						{
							return;
						}
						DateTime t = DateTime.Now.AddMinutes(-5.0);
						string[] array = files;
						foreach (string path in array)
						{
							try
							{
								if (File.GetLastAccessTime(path) < t)
								{
									File.Delete(path);
								}
							}
							catch (IOException)
							{
							}
							catch (UnauthorizedAccessException)
							{
							}
						}
					}
					finally
					{
						try
						{
							TempFileCleanupSem.Release();
						}
						catch (SemaphoreFullException)
						{
						}
					}
				}
			});
		}

		/// <summary>
		/// In .Net 4.0 and above, in some cases, ConfigurationErrorsException
		/// will not be catch without the HandleProcessCorruptedStateExceptions
		/// and SecurityCritical attributes. The method should not be async.
		///
		/// See Bug 172014 and
		/// https://msdn.microsoft.com/en-us/magazine/dd419661.aspx
		/// </summary>
		/// <param name="requestUrl"></param>
		/// <returns></returns>
		[HandleProcessCorruptedStateExceptions]
		[SecurityCritical]
		private static HttpWebRequest CreateHttpRequest(string requestUrl)
		{
			try
			{
				return (HttpWebRequest)WebRequest.Create(requestUrl);
			}
			catch (ConfigurationErrorsException)
			{
				return null;
			}
		}

		/// <summary>
		/// This is the HTTP-facing method in the class. It essentially performs all functions of
		///     1. sending an HTTP request for the file
		///     2. if the response is from the server, ensuring that it is added to the IE cache
		///     3. additonal logic to cache error responses in the IE cache. By default error
		///        responses are not cached.
		/// </summary>
		/// <param name="requestUrl"></param>
		/// <param name="requestTimeoutMillis"></param>
		/// <param name="cachePolicy">Specifies chache policy to use when sending the request</param>
		/// <returns>Result of the file lookup. See <see cref="T:Coding4Fun.VisualStudio.RemoteControl.GetFileResult" /> for details.</returns>
		private async Task<GetFileResult> GetFile(string requestUrl, int requestTimeoutMillis, HttpRequestCachePolicy cachePolicy)
		{
			HttpWebRequest request = CreateHttpRequest(requestUrl);
			if (request == null)
			{
				return new GetFileResult
				{
					ErrorMessage = "Create HTTP Request Error",
					Code = HttpStatusCode.Unused
				};
			}
			request.Timeout = requestTimeoutMillis;
			request.CachePolicy = (cachePolicy ?? new HttpRequestCachePolicy(HttpCacheAgeControl.MaxStale, TimeSpan.MaxValue));
			request.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);
			HttpWebResponse resp = null;
			WebException exception = null;
			try
			{
				Task<WebResponse> requestTask = request.GetResponseAsync();
				bool shouldAbort = false;
				try
				{
					if (await Task.WhenAny(requestTask, Task.Delay(requestTimeoutMillis, cancellationTokenSource.Token)).ConfigureAwait(false) != requestTask)
					{
						shouldAbort = true;
					}
				}
				catch (Exception)
				{
					shouldAbort = true;
				}
				if (shouldAbort)
				{
					request.Abort();
					requestTask.SwallowException();
					return new GetFileResult
					{
						ErrorMessage = "Request timed out",
						Code = HttpStatusCode.Unused
					};
				}
				_ = resp;
				resp = (HttpWebResponse)(await requestTask.ConfigureAwait(false));
			}
			catch (WebException ex2)
			{
				exception = ex2;
			}
			catch (Exception ex3)
			{
				return new GetFileResult
				{
					ErrorMessage = "Unknown Exception: " + ex3.Message,
					Code = HttpStatusCode.Unused
				};
			}
			string errorMessage2 = null;
			if (exception != null)
			{
				WebExceptionStatus status = exception.Status;
				if (status != WebExceptionStatus.ProtocolError)
				{
					errorMessage2 = string.Format(CultureInfo.InvariantCulture, "Non-Protocol Error {0}", new object[1]
					{
						Enum.GetName(typeof(WebExceptionStatus), exception.Status)
					});
					return new GetFileResult
					{
						ErrorMessage = errorMessage2,
						Code = HttpStatusCode.Unused
					};
				}
				resp = (exception.Response as HttpWebResponse);
				errorMessage2 = string.Format(CultureInfo.InvariantCulture, "Protocol Error {0}", new object[1]
				{
					Enum.GetName(typeof(HttpStatusCode), resp.StatusCode)
				});
			}
			if (resp != null)
			{
				int? ageInSeconds = ExtractAgeHeaderValue(resp);
				Stream stream = null;
				bool shouldDisposeResponseStream = false;
				try
				{
					if (!resp.IsFromCache)
					{
						shouldDisposeResponseStream = true;
						switch (resp.StatusCode)
						{
						case HttpStatusCode.OK:
							stream = await Task.Run(() => CopyToFileStream(resp.GetResponseStream())).ConfigureAwait(false);
							break;
						case HttpStatusCode.NotFound:
							if (Platform.IsWindows)
							{
								WinINetHelper.WriteErrorResponseToCache(requestUrl, HttpStatusCode.NotFound);
							}
							break;
						}
					}
					return new GetFileResult
					{
						Code = resp.StatusCode,
						RespStream = ((resp.StatusCode == HttpStatusCode.OK) ? (stream ?? resp.GetResponseStream()) : null),
						AgeSeconds = ageInSeconds,
						IsFromCache = resp.IsFromCache,
						ErrorMessage = errorMessage2
					};
				}
				catch (UnauthorizedAccessException ex4)
				{
					shouldDisposeResponseStream = true;
					errorMessage2 = ex4.Message;
				}
				catch (OperationCanceledException)
				{
					shouldDisposeResponseStream = true;
					errorMessage2 = "Download was cancelled by caller";
				}
				catch (ObjectDisposedException)
				{
					shouldDisposeResponseStream = true;
					errorMessage2 = "Download was cancelled by caller";
				}
				catch (WebException)
				{
					shouldDisposeResponseStream = true;
					errorMessage2 = "Reading HTTP response stream throws an WebException";
				}
				finally
				{
					if (shouldDisposeResponseStream || resp.StatusCode != HttpStatusCode.OK)
					{
						resp.GetResponseStream()?.Dispose();
					}
				}
				return new GetFileResult
				{
					ErrorMessage = errorMessage2,
					Code = HttpStatusCode.Unused
				};
			}
			throw new InvalidOperationException("WebException is protocol, but response is null", exception);
		}

		/// <summary>
		/// Copies all data in <paramref name="s" /> to a new File Stream based on a temp file.
		/// Note that we cannot use a MemoryStream here because responses may be of large size and we do not
		/// want to risk reading the entire body into memory at one time.
		/// </summary>
		/// <param name="s">Original Stream</param>
		/// <returns>A new file stream</returns>
		private FileStream CopyToFileStream(Stream s)
		{
			DirectoryInfo directoryInfo = Directory.CreateDirectory(TempFileDir);
			try
			{
				DirectorySecurity accessControl = directoryInfo.GetAccessControl();
				SecurityIdentifier identity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
				accessControl.AddAccessRule(new FileSystemAccessRule(identity, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
				directoryInfo.SetAccessControl(accessControl);
			}
			catch
			{
			}
			try
			{
				DirectorySecurity accessControl2 = directoryInfo.GetAccessControl();
				SecurityIdentifier identity2 = new SecurityIdentifier("S-1-15-2-1");
				accessControl2.AddAccessRule(new FileSystemAccessRule(identity2, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
				directoryInfo.SetAccessControl(accessControl2);
			}
			catch
			{
			}
			FileStream fileStream = File.Create(Path.Combine(TempFileDir, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)));
			try
			{
				byte[] array = new byte[10000];
				int num = 0;
				do
				{
					cancellationTokenSource.Token.ThrowIfCancellationRequested();
					num = s.Read(array, 0, array.Length);
					fileStream.Write(array, 0, num);
				}
				while (num != 0);
				fileStream.Position = 0L;
				return fileStream;
			}
			catch
			{
				fileStream.Dispose();
				throw;
			}
			finally
			{
				StartTempFileCleanup();
			}
		}
	}
}
