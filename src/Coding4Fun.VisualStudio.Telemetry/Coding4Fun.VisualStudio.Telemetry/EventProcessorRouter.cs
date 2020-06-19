using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Router serves for route processed event to the channels.
	/// It is fullfilled by the routed arguments specified for the channels.
	/// Also it is possible to disable/enable channel for the specified event.
	/// </summary>
	internal sealed class EventProcessorRouter : TelemetryDisposableObject, IEventProcessorRouter, IDisposable
	{
		/// <summary>
		/// Structure for the keeping route information
		/// </summary>
		private struct RouteInformation
		{
			public bool IsChannelAvailable;

			public bool IsChannelDisabled;

			public List<ITelemetryManifestRouteArgs> RouteArgs;
		}

		private const string ChannelUsedProperty = "ChannelUsed";

		private readonly IPersistentPropertyBag persistentPropertyBag;

		private Stopwatch disposeLatencyTimer;

		private RouteInformation[] routeInformation = new RouteInformation[0];

		private Dictionary<string, int> channelMapping = new Dictionary<string, int>();

		private ConcurrentBag<ISessionChannel> channelList = new ConcurrentBag<ISessionChannel>();

		public EventProcessorRouter(IPersistentPropertyBag persistentPropertyBag)
		{
			CodeContract.RequiresArgumentNotNull<IPersistentPropertyBag>(persistentPropertyBag, "persistentPropertyBag");
			this.persistentPropertyBag = persistentPropertyBag;
		}

		/// <summary>
		/// Reset all isChannelAvailable array to false
		/// </summary>
		public void Reset()
		{
			for (int num = routeInformation.Length - 1; num >= 0; num--)
			{
				routeInformation[num].IsChannelAvailable = false;
				routeInformation[num].IsChannelDisabled = false;
				routeInformation[num].RouteArgs.Clear();
			}
		}

		/// <summary>
		/// Check, whether we have scheduled routing for the specific channel.
		/// </summary>
		/// <param name="channelId"></param>
		/// <param name="routeArguments"></param>
		/// <returns></returns>
		public bool TryGetRouteArgument(string channelId, out IEnumerable<ITelemetryManifestRouteArgs> routeArguments)
		{
			if (channelMapping.TryGetValue(channelId, out int value) && routeInformation[value].IsChannelAvailable)
			{
				routeArguments = routeInformation[value].RouteArgs;
				return true;
			}
			routeArguments = null;
			return false;
		}

		/// <summary>
		/// In route action we fill routing argument with the specific value
		/// </summary>
		/// <param name="channelId"></param>
		/// <param name="routeArguments"></param>
		/// <returns></returns>
		public bool TryAddRouteArgument(string channelId, ITelemetryManifestRouteArgs routeArguments)
		{
			if (channelMapping.TryGetValue(channelId, out int value))
			{
				routeInformation[value].IsChannelAvailable = true;
				routeInformation[value].RouteArgs.Add(routeArguments);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Disable specific channel
		/// </summary>
		/// <param name="channelId"></param>
		public void DisableChannel(string channelId)
		{
			if (channelMapping.TryGetValue(channelId, out int value))
			{
				routeInformation[value].IsChannelDisabled = true;
			}
		}

		/// <summary>
		/// Check, whether channel is disabled
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		public bool IsChannelDisabled(string channelId)
		{
			if (channelMapping.TryGetValue(channelId, out int value))
			{
				return routeInformation[value].IsChannelDisabled;
			}
			return false;
		}

		/// <summary>
		/// Add session channel
		/// </summary>
		/// <param name="channel"></param>
		public void AddChannel(ISessionChannel channel)
		{
			CodeContract.RequiresArgumentNotNull<ISessionChannel>(channel, "channel");
			channelList.Add(channel);
			OnUpdateChannelList();
		}

		/// <summary>
		/// Post cooked event to the available channels. In case if event is dropped (isDropped == true)
		/// we still want to post this event to the DevChannel if any. Dev channel is used for the testing
		/// purposes.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="sessionId"></param>
		/// <param name="isDropped"></param>
		public void RouteEvent(TelemetryEvent telemetryEvent, string sessionId, bool isDropped)
		{
			List<Tuple<ISessionChannel, IEnumerable<ITelemetryManifestRouteArgs>>> list = new List<Tuple<ISessionChannel, IEnumerable<ITelemetryManifestRouteArgs>>>();
			foreach (ISessionChannel channel in channelList)
			{
				bool flag = (channel.Properties & ChannelProperties.DevChannel) != 0;
				if (flag || (!isDropped && !IsChannelDisabled(channel.ChannelId)))
				{
					IEnumerable<ITelemetryManifestRouteArgs> routeArguments = null;
					bool flag2 = TryGetRouteArgument(channel.ChannelId, out routeArguments);
					if ((flag | flag2) || (channel.Properties & (ChannelProperties.Default | ChannelProperties.Test | ChannelProperties.DevChannel)) != 0)
					{
						if (!channel.IsStarted)
						{
							channel.Start(sessionId);
						}
						list.Add(Tuple.Create(channel, routeArguments));
					}
				}
			}
			if (list.Count > 0)
			{
				telemetryEvent.Properties["Reserved.ChannelUsed"] = StringExtensions.Join(from item in list
					where (item.Item1.Properties & ChannelProperties.DevChannel) == 0
					select item.Item1.TransportUsed, ",");
				foreach (Tuple<ISessionChannel, IEnumerable<ITelemetryManifestRouteArgs>> item in list)
				{
					if (item.Item2 == null)
					{
						item.Item1.PostEvent(telemetryEvent);
					}
					else
					{
						item.Item1.PostEvent(telemetryEvent, item.Item2);
					}
				}
			}
		}

		/// <summary>
		/// One more way to dispose router asynchronously.
		/// In that case it asks channels to flush everything to the Network asynchronously if channel supported such method.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Task for wait on</returns>
		public async Task DisposeAndTransmitAsync(CancellationToken token)
		{
			DisposeStart();
			List<Task> list = new List<Task>();
			foreach (ISessionChannel channel in channelList)
			{
				if (channel is IDisposeAndTransmit)
				{
					list.Add(((IDisposeAndTransmit)channel).DisposeAndTransmitAsync(token));
				}
			}
			foreach (ISessionChannel channel2 in channelList)
			{
				if (!(channel2 is IDisposeAndTransmit) && channel2 is IDisposable)
				{
					((IDisposable)channel2).Dispose();
				}
			}
			await Task.WhenAll(list).ConfigureAwait(false);
			DisposeEnd();
		}

		/// <summary>
		/// Dispose all channels
		/// </summary>
		protected override void DisposeManagedResources()
		{
			DisposeStart();
			foreach (ISessionChannel channel in channelList)
			{
				if (channel is IDisposable)
				{
					((IDisposable)channel).Dispose();
				}
			}
			DisposeEnd();
		}

		private void DisposeStart()
		{
			base.DisposeManagedResources();
			disposeLatencyTimer = new Stopwatch();
			disposeLatencyTimer.Start();
		}

		private void DisposeEnd()
		{
			persistentPropertyBag.SetProperty("VS.TelemetryApi.ChannelsDisposeLatency", (int)disposeLatencyTimer.ElapsedMilliseconds);
			RemoveAllChannels();
		}

		/// <summary>
		/// Remove all channels
		/// </summary>
		private void RemoveAllChannels()
		{
			channelList = new ConcurrentBag<ISessionChannel>();
			OnUpdateChannelList();
		}

		/// <summary>
		/// When channels set is updated we call this method to re-init all arrays
		/// </summary>
		private void OnUpdateChannelList()
		{
			routeInformation = new RouteInformation[channelList.Count()];
			channelMapping = new Dictionary<string, int>();
			int num = 0;
			foreach (ISessionChannel channel in channelList)
			{
				routeInformation[num].RouteArgs = new List<ITelemetryManifestRouteArgs>();
				channelMapping[channel.ChannelId] = num++;
			}
		}
	}
}
