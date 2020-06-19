using System;

namespace Coding4Fun.VisualStudio.Telemetry.Notification
{
	/// <summary>
	/// Runtime <see cref="T:Coding4Fun.VisualStudio.Telemetry.Notification.ITelemetryNotificationProvider" />, using <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryService" />.
	/// </summary>
	internal class TelemetryNotificationProvider : ITelemetryNotificationProvider
	{
		private ITelemetryTestChannel channel;

		private TelemetrySession telemetrySession;

		public TelemetryNotificationProvider(TelemetrySession session)
		{
			telemetrySession = session;
		}

		public void AttachChannel(ITelemetryTestChannel channel)
		{
			this.channel = channel;
			if (telemetrySession == null)
			{
				telemetrySession = TelemetryService.DefaultSession;
			}
			TelemetrySession obj = telemetrySession;
			obj.RawTelemetryEventReceived = (EventHandler<TelemetryTestChannelEventArgs>)Delegate.Combine(obj.RawTelemetryEventReceived, new EventHandler<TelemetryTestChannelEventArgs>(channel.OnPostEvent));
		}

		public void DetachChannel(ITelemetryTestChannel channel)
		{
			if (telemetrySession != null)
			{
				TelemetrySession obj = telemetrySession;
				obj.RawTelemetryEventReceived = (EventHandler<TelemetryTestChannelEventArgs>)Delegate.Remove(obj.RawTelemetryEventReceived, new EventHandler<TelemetryTestChannelEventArgs>(channel.OnPostEvent));
			}
			this.channel = null;
		}

		public void PostFaultEvent(string eventName, string description, Exception exception)
		{
			TelemetryService.DefaultSession.PostFault(eventName, description, exception);
		}
	}
}
