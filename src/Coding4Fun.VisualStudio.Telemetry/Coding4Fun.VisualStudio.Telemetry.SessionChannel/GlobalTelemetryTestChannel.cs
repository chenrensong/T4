using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Global test channel for all sessions. All events from all sessions are captured here.
	/// </summary>
	internal sealed class GlobalTelemetryTestChannel : ISessionChannel
	{
		private static readonly GlobalTelemetryTestChannel PrivateInstance = new GlobalTelemetryTestChannel();

		public string ChannelId => "developerchannel";

		public bool IsStarted => true;

		public ChannelProperties Properties
		{
			get
			{
				return ChannelProperties.DevChannel;
			}
			set
			{
			}
		}

		public string TransportUsed => string.Empty;

		public static GlobalTelemetryTestChannel Instance => PrivateInstance;

		public event EventHandler<TelemetryTestChannelEventArgs> EventPosted;

		public void PostEvent(TelemetryEvent telemetryEvent)
		{
			EventHandler<TelemetryTestChannelEventArgs> eventPosted = this.EventPosted;
			if (eventPosted != null)
			{
				TelemetryTestChannelEventArgs e = new TelemetryTestChannelEventArgs
				{
					Event = telemetryEvent
				};
				eventPosted(this, e);
			}
		}

		public void ClearEventSubscribers()
		{
			this.EventPosted = null;
		}

		public void PostEvent(TelemetryEvent telemetryEvent, IEnumerable<ITelemetryManifestRouteArgs> args)
		{
			PostEvent(telemetryEvent);
		}

		public void Start(string sessionId)
		{
		}

		/// <summary>
		/// Private constructor to avoid create this object
		/// </summary>
		private GlobalTelemetryTestChannel()
		{
		}

		public override string ToString()
		{
			return ChannelId ?? "";
		}
	}
}
