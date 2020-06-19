using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Base AppInsights channel. Implements base operations on AI session channel
	/// </summary>
	internal abstract class BaseAppInsightsSessionChannel : TelemetryDisposableObject, ISessionChannel, IDisposeAndTransmit
	{
		protected readonly string InstrumentationKey;

		protected readonly string UserId;

		private readonly Lazy<string> transportUsed;

		private const string SequenceNumberPropertyName = "Reserved.SequenceNumber";

		/// <summary>
		/// Is channel already started. Prevent from start channel several times.
		/// </summary>
		private bool isChannelStarted;

		private IAppInsightsClientWrapper appInsightsClient;

		private int eventCounter;

		private ChannelProperties channelProperties;

		/// <summary>
		/// Gets the channel id, known by the concrete implementation.
		/// </summary>
		public abstract string ChannelId
		{
			get;
		}

		/// <summary>
		/// Gets used transport for the Asimov channel. Expected to be called after Start() method.
		/// In case it is called before Start() exception will be thrown.
		/// </summary>
		public string TransportUsed => transportUsed.Value;

		/// <summary>
		/// Gets or sets channel properties. It could restricts access to the channel.
		/// </summary>
		public ChannelProperties Properties
		{
			get
			{
				return channelProperties;
			}
			set
			{
				channelProperties = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether channel is started
		/// </summary>
		/// <returns></returns>
		public bool IsStarted => isChannelStarted;

		internal string IKey => InstrumentationKey;

		/// <summary>
		/// Gets the folder name for the Persistence storage
		/// </summary>
		protected string PersistenceFolderName => "vstel" + InstrumentationKey;

		/// <summary>
		/// Initialize BaseAppInsightsSessionChannel and calculate used transport.
		/// Calculate used transport by asking concrete implementation if specific transport is used.
		/// In case specific transport is not used transportUsed just channel id.
		/// In case specific transport is used (as for combined channel UTC/Vortex)
		/// transportUsed is channelid.specific transport
		/// </summary>
		/// <param name="instrumentationKey"></param>
		/// <param name="userId"></param>
		/// <param name="overridedClientWrapper"></param>
		/// <param name="defaultChannelProperties"></param>
		public BaseAppInsightsSessionChannel(string instrumentationKey, string userId, IAppInsightsClientWrapper overridedClientWrapper = null, ChannelProperties defaultChannelProperties = ChannelProperties.NotForUnitTest)
		{
			appInsightsClient = overridedClientWrapper;
			InstrumentationKey = instrumentationKey;
			UserId = userId;
			channelProperties = defaultChannelProperties;
			transportUsed = new Lazy<string>(delegate
			{
				CodeContract.RequiresArgumentNotNull<IAppInsightsClientWrapper>(appInsightsClient, "appInsightsClient");
				string text = ChannelId;
				if (appInsightsClient.TryGetTransport(out string str))
				{
					text = text + "." + str;
				}
				return text;
			});
		}

		/// <summary>
		/// Start session channel opens channel and make it ready to send events.
		/// </summary>
		/// <param name="sessionId"></param>
		public void Start(string sessionId)
		{
			if (!isChannelStarted)
			{
				isChannelStarted = true;
				if (appInsightsClient == null)
				{
					appInsightsClient = CreateAppInsightsClientWrapper();
				}
				appInsightsClient.Initialize(sessionId, UserId);
				return;
			}
			throw new InvalidOperationException("AppInsightsSessionChannel.Start must be called only once");
		}

		/// <summary>
		/// Transmit all internal buffers to the end-point and dispose channel.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns></returns>
		public async Task DisposeAndTransmitAsync(CancellationToken token)
		{
			base.DisposeManagedResources();
			if (appInsightsClient != null)
			{
				await appInsightsClient.DisposeAndTransmitAsync(token).ConfigureAwait(false);
				appInsightsClient = null;
			}
		}

		/// <summary>
		/// Posts a telemetry event.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		public void PostEvent(TelemetryEvent telemetryEvent)
		{
			RequiresNotDisposed();
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			EnsureChannelIsStarted();
			EventTelemetry eventTelemetry = new EventTelemetry(telemetryEvent.Name);
			eventTelemetry.Timestamp = telemetryEvent.PostTimestamp;
			foreach (KeyValuePair<string, object> item in telemetryEvent.Properties.Where((KeyValuePair<string, object> keyValue) => keyValue.Value != null))
			{
				if (TypeTools.IsNumericType(item.Value.GetType()))
				{
					double num = Convert.ToDouble(item.Value, null);
					if (!double.IsNaN(num) && !double.IsInfinity(num))
					{
						eventTelemetry.Metrics.Add(item.Key, num);
					}
				}
				else
				{
					eventTelemetry.Properties.Add(item.Key, item.Value.ToString());
				}
			}
			int num2 = Interlocked.Increment(ref eventCounter);
			eventTelemetry.Metrics["Reserved.SequenceNumber"] = num2;
			appInsightsClient.TrackEvent(eventTelemetry);
		}

		/// <summary>
		/// Post routed event
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="args"></param>
		public void PostEvent(TelemetryEvent telemetryEvent, IEnumerable<ITelemetryManifestRouteArgs> args)
		{
			PostEvent(telemetryEvent);
		}

		/// <summary>
		/// Obtain AppInsights client wrapper
		/// </summary>
		/// <returns></returns>
		protected abstract IAppInsightsClientWrapper CreateAppInsightsClientWrapper();

		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			if (appInsightsClient != null)
			{
				appInsightsClient.Dispose();
				appInsightsClient = null;
			}
		}

		private void EnsureChannelIsStarted()
		{
			if (!isChannelStarted)
			{
				throw new InvalidOperationException("AppInsightsSessionChannel.Start must be called before this method");
			}
		}
	}
}
