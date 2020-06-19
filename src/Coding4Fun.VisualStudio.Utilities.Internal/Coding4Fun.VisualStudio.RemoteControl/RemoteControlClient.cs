using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// Client for the Remote Control Service.
	/// This client polls the service for a single settings file and keeps it up-to-date in the local IE cache (which
	/// is per-user).
	/// Developers may call the <see cref="M:Coding4Fun.VisualStudio.RemoteControl.RemoteControlClient.ReadFile(Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale)" /> or <see cref="M:Coding4Fun.VisualStudio.RemoteControl.RemoteControlClient.ReadFileAsync(Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale)" /> to read settings file.
	/// </summary>
	public class RemoteControlClient : IRemoteControlClient, IDisposable
	{
		private static Action<string, IDictionary<string, object>> telemetryLogger = delegate
		{
		};

		private static Action<string, IDictionary<string, object>, IDictionary<string, object>> telemetryLogger2 = delegate(string eventName, IDictionary<string, object> properties, IDictionary<string, object> piiProperties)
		{
			telemetryLogger(eventName, properties);
		};

		private const int DefaultHTTPRequestTimeoutSeconds = 60;

		private const int DefaultPollingIntervalMins = 1380;

		private const int DefaultReadFileTelemetry = 1;

		private const int MinPollingIntervalMins = 5;

		private const int MaxHTTPRequestTimeoutSeconds = 60;

		private const int RemoteControlExplicitlyDisabled = 1;

		private readonly int maxRandomDownloadDelaySeconds = 15000;

		private readonly int httpRequestTimeoutSeconds;

		private readonly Timer cacheUpdateTimer;

		private readonly Random rand = new Random();

		private readonly IRemoteControlHTTPRequestor requestor;

		private readonly SemaphoreSlim updateMutex = new SemaphoreSlim(1, 1);

		private readonly CancellationTokenSource cancellationToken = new CancellationTokenSource();

		private readonly IFileReader fileReader;

		private readonly bool isDisabled;

		private bool isDisposed;

		/// <summary>
		/// Gets or sets an action which allows setting of a Telemetry Logger to
		/// collect telemetry for usage of the Remote Control.
		/// </summary>
		public static Action<string, IDictionary<string, object>> TelemetryLogger
		{
			get
			{
				return telemetryLogger;
			}
			set
			{
				value.RequiresArgumentNotNull("value");
				telemetryLogger = value;
			}
		}

		/// <summary>
		/// Gets or sets an action which allows setting of a Telemetry Logger to
		/// collect telemetry for usage of the Remote Control. Includes Pii-option.
		/// </summary>
		public static Action<string, IDictionary<string, object>, IDictionary<string, object>> TelemetryLogger2
		{
			get
			{
				return telemetryLogger2;
			}
			set
			{
				value.RequiresArgumentNotNull("value");
				telemetryLogger2 = value;
			}
		}

		/// <summary>
		/// Gets a full URL used to download to the file.
		/// </summary>
		public string FullUrl => Uri.FullUrl;

		/// <summary>
		/// Gets a polling Interval to check the file on the server. This setting also determines
		/// when a file in the local IE cache is considered stale.
		/// </summary>
		public int PollingIntervalMins
		{
			get;
		}

		internal RemoteControlUri Uri
		{
			get;
		}

		/// <summary>
		/// Creates the client and starts polling.
		/// </summary>
		/// <param name="hostId">HostId of the settings file (used to construct URL to the file:
		/// [baseUrl]/[hostId]/[relativePath]).</param>
		/// <param name="baseUrl">Base URL of the service e.g. https://az700632.vo.msecnd.net</param>
		/// <param name="relativePath">Relative path used to contruct the full URL to the file:
		/// [baseUrl]/[hostId]/[relativePath]</param>
		/// <param name="pollingIntervalMins">Optional. Default = 1440 minutes (24 hours). Min allowed = 5 minutes.
		/// Polling Interval (in minutes) to check the file on the server when the last request to the server
		/// succeeded.</param>
		/// <param name="theHttpRequestTimeoutSeconds">Optional. Default = 60 seconds. Maximum allowed = 60 seconds.
		/// HTTP request timeout used.</param>
		/// <param name="overrideReadFileTelemetryFrequency">Optional. Allows to set how often to send successful
		/// ReadFile telemetry, to prevent noise events Default = 1 (meaning post ReadFile telemetry every time). Min
		/// allowed is 1.</param>
		public RemoteControlClient(string hostId, string baseUrl, string relativePath, int pollingIntervalMins = 1380, int theHttpRequestTimeoutSeconds = 60, int overrideReadFileTelemetryFrequency = 1)
			: this(new RegistryTools(), hostId, baseUrl, relativePath, pollingIntervalMins)
		{
			if (!Uri.IsLocalFile)
			{
				httpRequestTimeoutSeconds = Math.Min(60, theHttpRequestTimeoutSeconds);
				if (!isDisabled)
				{
					requestor = new RemoteControlHTTPRequestor(FullUrl, httpRequestTimeoutSeconds * 1000);
					cacheUpdateTimer = new Timer(delegate(object o)
					{
						CacheUpdateTimerCallback(o, cancellationToken.Token).SwallowException();
					});
					cacheUpdateTimer.Change(0, -1);
				}
			}
			else
			{
				fileReader = new FileReader(FullUrl);
			}
		}

		/// <summary>
		/// Only for testing. Testing "fake" IE Cache or "fake" server response
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="theRegistryTools"></param>
		/// <param name="hostId"></param>
		/// <param name="baseUrl"></param>
		/// <param name="relativePath"></param>
		/// <param name="pollingIntervalMins"></param>
		/// <param name="httpRequestTimeoutSeconds"></param>
		/// <param name="maxRandomDownloadDelaySeconds"></param>
		internal RemoteControlClient(IRemoteControlHTTPRequestor requestor, IRegistryTools theRegistryTools, string hostId, string baseUrl, string relativePath, int pollingIntervalMins = 1380, int httpRequestTimeoutSeconds = 60, int maxRandomDownloadDelaySeconds = 0)
			: this(theRegistryTools, hostId, baseUrl, relativePath, pollingIntervalMins)
		{
			this.maxRandomDownloadDelaySeconds = maxRandomDownloadDelaySeconds;
			this.requestor = requestor;
			cacheUpdateTimer = new Timer(delegate(object o)
			{
				CacheUpdateTimerCallback(o, cancellationToken.Token).SwallowException();
			}, null, -1, -1);
		}

		/// <summary>
		/// Only for testing. Testing local functionality
		/// </summary>
		/// <param name="theRegistryTools"></param>
		/// <param name="theFileReader"></param>
		/// <param name="hostId"></param>
		/// <param name="baseUrl"></param>
		/// <param name="relativePath"></param>
		internal RemoteControlClient(IRegistryTools theRegistryTools, IFileReader theFileReader, string hostId, string baseUrl, string relativePath)
			: this(theRegistryTools, hostId, baseUrl, relativePath, 1380)
		{
			fileReader = theFileReader;
		}

		private RemoteControlClient(IRegistryTools registryTools, string hostId, string baseUrl, string relativePath, int pollingIntervalMins)
		{
			Uri = RemoteControlUri.Create(registryTools, hostId, baseUrl, relativePath);
			PollingIntervalMins = Math.Max(5, pollingIntervalMins);
			int num = 0;
			try
			{
				num = Convert.ToInt32(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\RemoteControl", "TurnOffSwitch", 0));
			}
			catch
			{
			}
			isDisabled = (num == 1);
		}

		/// <summary>
		/// Reads the settings file based on the <paramref name="staleBehavior" /> specified.
		/// </summary>
		/// <param name="staleBehavior">See <see cref="T:Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale" /> for details about each possible setting.
		/// In most cases use the BehaviorOnStale.ReturnStale setting.</param>
		/// <returns>A Stream that can be used to read the setting file. !Callers must call Dispose on this stream
		/// object returned. Or Null is returned in case of failure to get the file (or if server returned
		/// NotFound).</returns>
		public Stream ReadFile(BehaviorOnStale staleBehavior)
		{
			if (isDisabled)
			{
				return null;
			}
			if (isDisposed)
			{
				throw new ObjectDisposedException("RemoteControlClient");
			}
			return ReadFileAsync(staleBehavior).Result;
		}

		/// <summary>
		/// Reads the settings file based on the <paramref name="staleBehavior" /> specified. This is the Async version
		/// of ReadFile method.
		/// </summary>
		/// <param name="staleBehavior">See <see cref="T:Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale" /> for details about each possible setting.
		/// In most cases use the BehaviorOnStale.ReturnStale setting.</param>
		/// <returns>A Stream that can be used to read the setting file. !Callers must call Dispose on this stream
		/// object returned. Or Null is returned in case of failure to get the file (or if server returned
		/// NotFound).</returns>
		public async Task<Stream> ReadFileAsync(BehaviorOnStale staleBehavior)
		{
			if (isDisabled)
			{
				return null;
			}
			if (isDisposed)
			{
				throw new ObjectDisposedException("RemoteControlClient");
			}
			if (Uri.IsLocalFile)
			{
				return (await ReadFileFromLocalAsync().ConfigureAwait(false)).RespStream;
			}
			switch (staleBehavior)
			{
			case BehaviorOnStale.ReturnStale:
				return (await GetFileAndInstrumentAsync().ConfigureAwait(false)).RespStream;
			case BehaviorOnStale.ReturnNull:
			{
				GetFileResult getFileResult = await GetFileAndInstrumentAsync().ConfigureAwait(false);
				if (IsStale(getFileResult))
				{
					return getFileResult.RespStream;
				}
				getFileResult.Dispose();
				return null;
			}
			case BehaviorOnStale.ForceDownload:
				return (await GetFileAndInstrumentAsync(true).ConfigureAwait(false)).RespStream;
			default:
				return null;
			}
		}

		/// <summary>
		/// Disposes of client
		/// </summary>
		public void Dispose()
		{
			if (!isDisposed)
			{
				if (cacheUpdateTimer != null)
				{
					cacheUpdateTimer.Dispose();
				}
				cancellationToken.Cancel();
				if (requestor != null)
				{
					requestor.Cancel();
				}
				isDisposed = true;
			}
		}

		internal bool RunUpdateFileMethod()
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException("RemoteControlClient");
			}
			return EnsureFileIsUpToDateAsync(new CancellationToken(false)).Result;
		}

		private async Task<GetFileResult> ReadFileFromLocalAsync()
		{
			return await Task.Run(delegate
			{
				GetFileResult getFileResult = new GetFileResult
				{
					Code = HttpStatusCode.Unused
				};
				try
				{
					getFileResult.RespStream = fileReader.ReadFile();
					getFileResult.Code = HttpStatusCode.OK;
					return getFileResult;
				}
				catch (ArgumentException)
				{
					getFileResult.ErrorMessage = "File path contains invalid characters";
					return getFileResult;
				}
				catch (IOException)
				{
					getFileResult.ErrorMessage = "IO exception reading file";
					return getFileResult;
				}
				catch (UnauthorizedAccessException)
				{
					getFileResult.ErrorMessage = "Could not access file for reading";
					return getFileResult;
				}
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// Callback method for the update timer.
		/// </summary>
		/// <param name="stateInfo">Dummy</param>
		/// <param name="token">Cancellation token</param>
		/// <returns></returns>
		private async Task CacheUpdateTimerCallback(object stateInfo, CancellationToken token)
		{
			try
			{
				await EnsureFileIsUpToDateAsync(token).ConfigureAwait(false);
			}
			finally
			{
				if (!isDisposed)
				{
					try
					{
						cacheUpdateTimer.Change(PollingIntervalMins * 60 * 1000, -1);
					}
					catch (ObjectDisposedException)
					{
					}
				}
			}
		}

		/// <summary>
		/// Determines if a local IE cache copy of the file is up-to-date. If no cached copy is available or the
		/// cached copy is not up-to-date, a request is made to the server to download or revalidate the file. The
		/// result of the server request is cached.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token to cancel waiting of operation</param>
		/// <returns>Returns True if the copy in the IE cache is up-to-date by the end of the method. False in case of
		/// failures that prevent updating file.</returns>
		private async Task<bool> EnsureFileIsUpToDateAsync(CancellationToken cancellationToken)
		{
			if (isDisabled)
			{
				return false;
			}
			try
			{
				await updateMutex.WaitAsync(cancellationToken).ConfigureAwait(false);
				for (int i = 1; i <= 2; i++)
				{
					using (GetFileResult getFileResult = await GetFileAndInstrumentAsync().ConfigureAwait(false))
					{
						if (IsStale(getFileResult))
						{
							return getFileResult.IsSuccessStatusCode;
						}
					}
					if (await requestor.LastServerRequestErrorSecondsAgoAsync().ConfigureAwait(false) < PollingIntervalMins * 60)
					{
						return false;
					}
					if (i < 2)
					{
						await Task.Delay(rand.Next(0, maxRandomDownloadDelaySeconds), cancellationToken).ConfigureAwait(false);
					}
				}
				using (GetFileResult getFileResult2 = await GetFileAndInstrumentAsync(true).ConfigureAwait(false))
				{
					return getFileResult2.IsSuccessStatusCode;
				}
			}
			finally
			{
				try
				{
					updateMutex.Release();
				}
				catch (SemaphoreFullException)
				{
				}
			}
		}

		/// <summary>
		/// Get a file and send telemetry events.
		/// </summary>
		/// <param name="fromServer"></param>
		/// <returns></returns>
		private async Task<GetFileResult> GetFileAndInstrumentAsync(bool fromServer = false)
		{
			GetFileResult getFileResult = await(fromServer ? requestor.GetFileFromServerAsync() : requestor.GetFileFromCacheAsync()).ConfigureAwait(false);
			if (fromServer && (!getFileResult.IsFromCache || !getFileResult.IsSuccessStatusCode))
			{
				InstrumentGetFile(getFileResult);
			}
			return getFileResult;
		}

		/// <summary>
		/// Instruments the usage of GetFileFromCache and GetFileFromServer.
		/// </summary>
		/// <param name="fileResult"></param>
		private void InstrumentGetFile(GetFileResult fileResult)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"VS.RemoteControl.DownloadFile.FullUrl",
					FullUrl
				},
				{
					"VS.RemoteControl.DownloadFile.IsSuccess",
					fileResult.IsSuccessStatusCode
				},
				{
					"VS.RemoteControl.DownloadFile.HttpRequestTimeoutInSecs",
					httpRequestTimeoutSeconds
				},
				{
					"VS.RemoteControl.DownloadFile.IsFromCache",
					fileResult.IsFromCache
				},
				{
					"VS.RemoteControl.DownloadFile.PollingIntervalInMins",
					PollingIntervalMins
				}
			};
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			if (fileResult.IsSuccessStatusCode)
			{
				dictionary.Add("VS.RemoteControl.DownloadFile.IsNotFound", fileResult.Code == HttpStatusCode.NotFound);
				if (fileResult.RespStream != null)
				{
					dictionary.Add("VS.RemoteControl.DownloadFile.StreamSize", fileResult.RespStream.Length);
				}
				if (fileResult.AgeSeconds.HasValue)
				{
					dictionary.Add("VS.RemoteControl.DownloadFile.AgeInSecs", fileResult.AgeSeconds.Value);
				}
			}
			else
			{
				if (fileResult.Code != HttpStatusCode.Unused)
				{
					dictionary.Add("VS.RemoteControl.DownloadFile.ErrorCode", Enum.GetName(typeof(HttpStatusCode), fileResult.Code));
				}
				dictionary2.Add("VS.RemoteControl.DownloadFile.ErrorMessage", fileResult.ErrorMessage);
			}
			TelemetryLogger2("VS/RemoteControl/DownloadFile", dictionary, dictionary2);
		}

		private bool IsStale(GetFileResult fileResult)
		{
			if (fileResult.IsFromCache && fileResult.AgeSeconds.HasValue)
			{
				return fileResult.AgeSeconds <= PollingIntervalMins * 60;
			}
			return false;
		}
	}
}
