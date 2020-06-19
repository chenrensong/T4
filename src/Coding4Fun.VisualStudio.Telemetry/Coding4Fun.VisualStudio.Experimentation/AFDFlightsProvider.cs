using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Provides flights from the AFD (Azure Front Door):
	/// - build and send request to the AFD every 30 minutes;
	/// - parse response and update flights if necessary;
	/// - use local storage to keep flights from the previous session.
	/// </summary>
	internal sealed class AFDFlightsProvider : CachedRemotePollerFlightsProviderBase<ActiveFlightsData>
	{
		private const int DefaultPollingIntervalInSecs = 1800000;

		private const int DefaultRequestTimeout = 60000;

		private const string DefaultGetMethod = "GET";

		private const string DefaultContentType = "application/json";

		private const string DefaultUrl = "https://visualstudio-devdiv-c2s.msedge.net/ab";

		private static readonly HttpRequestCachePolicy DefaultCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

		/// <summary>
		/// Edge Endpoint only allows alphanumeric and dashes. We will use this regular
		/// expression to replace all non allowed characters with dashes.
		/// </summary>
		internal static readonly Regex RegExpression = new Regex("[^a-zA-Z0-9-]");

		private readonly string flightsKey;

		private readonly IExperimentationFilterProvider filterProvider;

		private readonly IHttpWebRequestFactory httpWebRequestFactory;

		public AFDFlightsProvider(IKeyValueStorage keyValueStorage, string flightsKey, IFlightsStreamParser flightsStreamParser, IExperimentationFilterProvider filterProvider, IHttpWebRequestFactory httpWebRequestFactory)
			: base(keyValueStorage, flightsStreamParser, 1800000)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(flightsKey, "flightsKey");
			CodeContract.RequiresArgumentNotNull<IExperimentationFilterProvider>(filterProvider, "filterProvider");
			CodeContract.RequiresArgumentNotNull<IHttpWebRequestFactory>(httpWebRequestFactory, "httpWebRequestFactory");
			this.filterProvider = filterProvider;
			this.flightsKey = flightsKey;
			this.httpWebRequestFactory = httpWebRequestFactory;
		}

		protected override void InternalDispose()
		{
		}

		protected override async Task<Stream> SendRemoteRequestInternalAsync()
		{
			IHttpWebRequest httpWebRequest = httpWebRequestFactory.Create("https://visualstudio-devdiv-c2s.msedge.net/ab");
			httpWebRequest.Method = "GET";
			httpWebRequest.CachePolicy = DefaultCachePolicy;
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.AddHeaders(GetAllFilters());
			Stream stream = null;
			try
			{
				CancellationTokenSource tokenSource = new CancellationTokenSource();
				Task.Delay(60000).ContinueWith(delegate
				{
					tokenSource.Cancel();
				});
				IHttpWebResponse httpWebResponse = await httpWebRequest.GetResponseAsync(tokenSource.Token).ConfigureAwait(false);
				if (httpWebResponse.ErrorCode != 0)
				{
					return stream;
				}
				stream = httpWebResponse.GetResponseStream();
				return stream;
			}
			catch
			{
				return stream;
			}
		}

		/// <summary>
		/// Build cach path key with filters. Key should look like:
		/// %BASE_PATH%\%ApplicationName%\%ApplicationVesion%\%Branch%\%flightsKey%
		/// </summary>
		/// <returns></returns>
		protected override string BuildFlightsKey()
		{
			List<string> list = new List<string>();
			AddIfNotEmpty(list, filterProvider.GetFilterValue(Filters.ApplicationName));
			AddIfNotEmpty(list, filterProvider.GetFilterValue(Filters.ApplicationVersion));
			AddIfNotEmpty(list, filterProvider.GetFilterValue(Filters.BranchBuildFrom));
			AddIfNotEmpty(list, flightsKey);
			return string.Join("\\", list);
		}

		private IEnumerable<KeyValuePair<string, string>> GetAllFilters()
		{
			yield return new KeyValuePair<string, string>("X-MSEdge-ClientID", ProcessFilterValue(filterProvider.GetFilterValue(Filters.UserId)));
			yield return new KeyValuePair<string, string>("X-MSEdge-AppID", ProcessFilterValue(filterProvider.GetFilterValue(Filters.ApplicationName)));
			yield return new KeyValuePair<string, string>("X-VisualStudio-Internal", ProcessFilterValue(filterProvider.GetFilterValue(Filters.IsInternal)));
			yield return new KeyValuePair<string, string>("X-VisualStudio-Build", ProcessFilterValue(filterProvider.GetFilterValue(Filters.ApplicationVersion)));
			yield return new KeyValuePair<string, string>("X-VisualStudio-BuildVersion", filterProvider.GetFilterValue(Filters.ApplicationVersion));
			yield return new KeyValuePair<string, string>("X-VisualStudio-Branch", ProcessFilterValue(filterProvider.GetFilterValue(Filters.BranchBuildFrom)));
			yield return new KeyValuePair<string, string>("X-VisualStudio-SKU", ProcessFilterValue(filterProvider.GetFilterValue(Filters.ApplicationSku)));
			yield return new KeyValuePair<string, string>("X-VisualStudio-ChannelId", ProcessFilterValue(filterProvider.GetFilterValue(Filters.ChannelId)));
		}

		private static string ProcessFilterValue(string value)
		{
			return RegExpression.Replace(value, "-");
		}

		private void AddIfNotEmpty(List<string> parts, string v)
		{
			if (!string.IsNullOrEmpty(v))
			{
				parts.Add(v);
			}
		}
	}
}
