using System;

namespace Coding4Fun.VisualStudio.Telemetry.Notification
{
	/// <summary>
	/// Telemetry notification service allows subscribers to be notified when telemetry event matching a specified rule is posted.
	/// </summary>
	public interface ITelemetryNotificationService
	{
		/// <summary>
		/// Subscribes to be notified when a telemetry event matching the specified filter is posted.
		/// </summary>
		/// <param name="eventMatch">The <see cref="T:Coding4Fun.VisualStudio.Telemetry.ITelemetryEventMatch" /> representing the filter rule for telemetry events.</param>
		/// <param name="handler">The handler to be invoked when a telemetry event matching the specified rule is posted.</param>
		/// <param name="singleNotification">Specifies whether to unsubscribe once a matching notification is raised.</param>
		/// <returns>A subscription ID that can be used to unsubscribe from notifications.</returns>
		/// <exception cref="T:System.ArgumentNullException">If <paramref name="eventMatch" /> is null, empty or white space.</exception>
		/// <exception cref="T:System.ArgumentNullException">If <paramref name="handler" /> is null.</exception>
		int Subscribe(ITelemetryEventMatch eventMatch, Action<TelemetryEvent> handler, bool singleNotification = true);

		/// <summary>
		/// Unsubscribes from the notification service.
		/// </summary>
		/// <param name="subscriptionId">The subscription ID to unsubscribe from.</param>
		void Unsubscribe(int subscriptionId);
	}
}
