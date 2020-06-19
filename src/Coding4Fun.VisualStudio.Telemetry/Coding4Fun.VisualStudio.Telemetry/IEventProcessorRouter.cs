using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IEventProcessorRouter : IDisposable
	{
		/// <summary>
		/// Reset all isChannelAvailable array to false
		/// </summary>
		void Reset();

		/// <summary>
		/// Check, whether we have scheduled routing for the specific channel.
		/// </summary>
		/// <param name="channelId"></param>
		/// <param name="routeArguments"></param>
		/// <returns></returns>
		bool TryGetRouteArgument(string channelId, out IEnumerable<ITelemetryManifestRouteArgs> routeArguments);

		/// <summary>
		/// In route action we fill routing argument with the specific value
		/// </summary>
		/// <param name="channelId"></param>
		/// <param name="routeArgument"></param>
		/// <returns></returns>
		bool TryAddRouteArgument(string channelId, ITelemetryManifestRouteArgs routeArgument);

		/// <summary>
		/// Disable specific channel
		/// </summary>
		/// <param name="channelId"></param>
		void DisableChannel(string channelId);

		/// <summary>
		/// Check, whether channel is disabled
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		bool IsChannelDisabled(string channelId);

		/// <summary>
		/// Add session channel
		/// </summary>
		/// <param name="channel"></param>
		void AddChannel(ISessionChannel channel);

		/// <summary>
		/// Post cooked event to the available channels
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="sessionId"></param>
		/// <param name="isDropped"></param>
		void RouteEvent(TelemetryEvent telemetryEvent, string sessionId, bool isDropped);

		/// <summary>
		/// One more way to dispose router asynchronously.
		/// In that case it asks channels to flush everything to the Network asynchronously if channel supported such method.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Task for wait on</returns>
		Task DisposeAndTransmitAsync(CancellationToken token);
	}
}
