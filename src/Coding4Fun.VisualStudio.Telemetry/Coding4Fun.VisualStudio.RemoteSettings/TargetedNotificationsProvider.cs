using Coding4Fun.VisualStudio.Telemetry.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class TargetedNotificationsProvider : TargetedNotificationsProviderBase
	{
		internal const string DefaultUrl = "https://go.microsoft.com/fwlink/?LinkId=690387";

		private const string DefaultContentType = "application/json";

		private const int DefaultRequestTimeout = 10000;

		private readonly IHttpWebRequestFactory webRequestFactory;

		private readonly ITargetedNotificationsParser notificationsParser;

		private readonly RemoteSettingsFilterProvider remoteSettingsFilterProvider;

		public override string Name => "TargetedNotifications";

		public TargetedNotificationsProvider(RemoteSettingsInitializer initializer)
			: base(initializer.CacheableRemoteSettingsStorageHandler, initializer)
		{
			webRequestFactory = initializer.HttpWebRequestFactory;
			notificationsParser = initializer.TargetedNotificationsParser;
			remoteSettingsFilterProvider = initializer.FilterProvider;
		}

		protected override async Task<ActionResponseBag> GetTargetedNotificationActionsAsync()
		{
			IEnumerable<string> previouslyCachedRuleIds = useCache ? notificationAndCourtesyCache.GetAllCachedRuleIds(cacheTimeoutMs) : Enumerable.Empty<string>();
			Stream stream = await SendTargetedNotificationsRequestAsync(previouslyCachedRuleIds).ConfigureAwait(false);
			if (stream != null)
			{
				try
				{
					ActionResponseBag actionResponseBag = await notificationsParser.ParseStreamAsync(stream).ConfigureAwait(false);
					if (useCache)
					{
						notificationAndCourtesyCache.MergeNewResponse(actionResponseBag, previouslyCachedRuleIds, cacheTimeoutMs);
					}
					return actionResponseBag;
				}
				catch (TargetedNotificationsException exception)
				{
					string eventName = "VS/Core/TargetedNotifications/ApiResponseParseFailure";
					Dictionary<string, object> additionalProperties = new Dictionary<string, object>
					{
						{
							"VS.Core.TargetedNotifications.ApiResponseMs",
							apiTimer?.ElapsedMilliseconds
						},
						{
							"VS.Core.TargetedNotifications.Iteration",
							queryIteration
						}
					};
					targetedNotificationsTelemetry.PostCriticalFault(eventName, "Failed to parse TN API response", exception, additionalProperties);
				}
			}
			return null;
		}

		private async Task<string> ResolveUrlRedirect(string url, CancellationToken token)
		{
			IHttpWebRequest httpWebRequest = webRequestFactory.Create(url);
			httpWebRequest.Method = "HEAD";
			httpWebRequest.AllowAutoRedirect = false;
			IHttpWebResponse httpWebResponse = await httpWebRequest.GetResponseAsync(token).ConfigureAwait(false);
			if (httpWebResponse.ErrorCode == ErrorCode.NoError && httpWebResponse.Headers != null)
			{
				string text = httpWebResponse.Headers["Location"];
				if (text != null)
				{
					return text;
				}
				throw new TargetedNotificationsException("Could not resolve url redirect (no location header was received)");
			}
			throw new TargetedNotificationsException($"Could not resolve url redirect ({httpWebResponse.ErrorCode}-{httpWebResponse.StatusCode}-{httpWebResponse.ExceptionCode})");
		}

		private async Task<Stream> SendTargetedNotificationsRequestAsync(IEnumerable<string> previouslyCachedRuleIds)
		{
			Stream stream = null;
			apiTimer = Stopwatch.StartNew();
			try
			{
				CancellationTokenSource tokenSource = new CancellationTokenSource();
				Task.Delay(10000).ContinueWith(delegate
				{
					tokenSource.Cancel();
				});
				CancellationToken cancelToken = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, cancellationTokenSource.Token).Token;
				string parameters = await BuildRequestParametersAsync(previouslyCachedRuleIds).ConfigureAwait(false);
				IHttpWebRequestFactory httpWebRequestFactory = webRequestFactory;
				IHttpWebRequest request = httpWebRequestFactory.Create(await ResolveUrlRedirect("https://go.microsoft.com/fwlink/?LinkId=690387", cancelToken).ConfigureAwait(false));
				request.ContentType = "application/json";
				request.Method = "POST";
				logger.LogVerbose("Sending request to TN backend", parameters);
				byte[] postData = Encoding.UTF8.GetBytes(parameters);
				request.ContentLength = postData.Length;
				Stream postStream = await request.GetRequestStreamAsync().ConfigureAwait(false);
				await postStream.WriteAsync(postData, 0, postData.Length, cancelToken).ConfigureAwait(false);
				postStream.Close();
				IHttpWebResponse httpWebResponse = await request.GetResponseAsync(cancelToken).ConfigureAwait(false);
				apiTimer.Stop();
				if (httpWebResponse.ErrorCode == ErrorCode.NoError)
				{
					stream = httpWebResponse.GetResponseStream();
				}
				else
				{
					string eventName = "VS/Core/TargetedNotifications/ApiRequestFailed";
					string description = "Request failed.";
					Dictionary<string, object> dictionary = new Dictionary<string, object>
					{
						{
							"VS.Core.TargetedNotifications.ErrorCode",
							httpWebResponse.ErrorCode
						},
						{
							"VS.Core.TargetedNotifications.ApiResponseMs",
							apiTimer?.ElapsedMilliseconds
						},
						{
							"VS.Core.TargetedNotifications.Iteration",
							queryIteration
						}
					};
					if (httpWebResponse.ErrorCode == ErrorCode.WebExceptionThrown)
					{
						dictionary.Add("VS.Core.TargetedNotifications.WebExceptionCode", httpWebResponse.ExceptionCode);
					}
					targetedNotificationsTelemetry.PostDiagnosticFault(eventName, description, null, dictionary);
				}
				return stream;
			}
			catch (Exception exception)
			{
				apiTimer.Stop();
				string eventName2 = "VS/Core/TargetedNotifications/ApiRequestException";
				string text = "Sending request to TN backend failed";
				Dictionary<string, object> additionalProperties = new Dictionary<string, object>
				{
					{
						"VS.Core.TargetedNotifications.ApiResponseMs",
						apiTimer?.ElapsedMilliseconds
					},
					{
						"VS.Core.TargetedNotifications.Iteration",
						queryIteration
					}
				};
				logger.LogError(text, exception);
				targetedNotificationsTelemetry.PostDiagnosticFault(eventName2, text, exception, additionalProperties);
				return stream;
			}
		}

		private async Task<string> BuildRequestParametersAsync(IEnumerable<string> previouslyCachedRuleIds)
		{
			ActionRequestParameters actionRequestParameters = new ActionRequestParameters
			{
				MachineId = HandleGuidParameter(remoteSettingsFilterProvider.GetMachineId()),
				UserId = HandleGuidParameter(remoteSettingsFilterProvider.GetUserId())
			};
			ActionRequestParameters actionRequestParameters2 = actionRequestParameters;
			actionRequestParameters2.VsoId = await remoteSettingsFilterProvider.GetVsIdAsync().ConfigureAwait(false);
			actionRequestParameters.Culture = remoteSettingsFilterProvider.GetCulture();
			actionRequestParameters.Version = remoteSettingsFilterProvider.GetApplicationVersion();
			actionRequestParameters.VsSku = remoteSettingsFilterProvider.GetVsSku();
			actionRequestParameters.NotificationsCount = remoteSettingsFilterProvider.GetNotificationsCount();
			actionRequestParameters.AppIdPackage = HandleGuidParameter(remoteSettingsFilterProvider.GetAppIdPackageGuid());
			actionRequestParameters.MacAddressHash = remoteSettingsFilterProvider.GetMacAddressHash();
			actionRequestParameters.ChannelId = remoteSettingsFilterProvider.GetChannelId();
			actionRequestParameters.ChannelManifestId = remoteSettingsFilterProvider.GetChannelManifestId();
			actionRequestParameters.ManifestId = remoteSettingsFilterProvider.GetManifestId();
			actionRequestParameters.OsType = remoteSettingsFilterProvider.GetOsType();
			actionRequestParameters.OsVersion = remoteSettingsFilterProvider.GetOsVersion();
			actionRequestParameters.ExeName = remoteSettingsFilterProvider.GetApplicationName();
			actionRequestParameters.IsInternal = HandleBoolParameter(remoteSettingsFilterProvider.GetIsUserInternal());
			actionRequestParameters.CachedRuleIds = previouslyCachedRuleIds;
			actionRequestParameters.SessionId = targetedNotificationsTelemetry.SessionId;
			return JsonConvert.SerializeObject((object)actionRequestParameters);
		}

		private static string HandleGuidParameter(Guid guid)
		{
			if (guid == default(Guid))
			{
				return string.Empty;
			}
			return guid.ToString();
		}

		private static int HandleBoolParameter(bool value)
		{
			if (value)
			{
				return 1;
			}
			return 0;
		}
	}
}
