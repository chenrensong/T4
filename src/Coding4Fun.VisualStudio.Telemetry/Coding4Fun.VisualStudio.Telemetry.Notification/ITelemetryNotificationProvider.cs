using System;

namespace Coding4Fun.VisualStudio.Telemetry.Notification
{
	/// <summary>
	/// Enables dependency injection for telemetry notification service.
	/// </summary>
	internal interface ITelemetryNotificationProvider
	{
		/// <summary>
		/// Attaches the notification service's test channel to receive telemetry events.
		/// </summary>
		/// <param name="channel">The notification service's test channel.</param>
		void AttachChannel(ITelemetryTestChannel channel);

		/// <summary>
		/// Detaches the notification service's test channel to stop receiving telemetry events.
		/// </summary>
		/// <param name="channel">The notification service's test channel.</param>
		void DetachChannel(ITelemetryTestChannel channel);

		/// <summary>
		/// Posts a fault telemetry event.
		/// </summary>
		void PostFaultEvent(string eventName, string description, Exception exception);
	}
}
