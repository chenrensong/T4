using System;

namespace Coding4Fun.VisualStudio.Telemetry.Notification
{
	/// <summary>
	/// A pass through test channel for the notification service.
	/// </summary>
	internal sealed class NotificationTelemetryChannel : ITelemetryTestChannel
	{
		private readonly Action<TelemetryEvent> handler;

		public NotificationTelemetryChannel(Action<TelemetryEvent> handler)
		{
			this.handler = handler;
		}

		public void OnPostEvent(object sender, TelemetryTestChannelEventArgs e)
		{
			if (handler != null && e != null)
			{
				handler(e.Event);
			}
		}
	}
}
