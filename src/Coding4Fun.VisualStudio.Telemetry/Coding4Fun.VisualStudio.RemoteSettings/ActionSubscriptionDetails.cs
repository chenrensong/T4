using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Telemetry.Notification;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Provides information about why a remote settings action subscription
	/// callback was invoked
	/// </summary>
	public sealed class ActionSubscriptionDetails
	{
		/// <summary>
		/// Well known name of the initial start trigger
		/// for triggered Targeted Notifications rules.
		/// </summary>
		public const string StartTrigger = "start";

		/// <summary>
		/// Well known name of the final stop trigger
		/// for triggered Targeted Notifications rules.
		/// </summary>
		public const string StopTrigger = "stop";

		/// <summary>
		/// Gets a value indicating whether the
		/// invoked trigger is set to trigger on
		/// every event match, or just once.
		/// </summary>
		public bool TriggerAlways
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets a value indicating whether the
		/// trigger was invoked directly after subscribing
		/// or not.
		/// </summary>
		public bool TriggerOnSubscribe
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the telemetry event that matched given
		/// filter conditions, if a telemetry notification
		/// subscription is the reason this callback was invoked
		/// </summary>
		public TelemetryEvent TelemetryEvent
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the trigger name being invoked
		/// </summary>
		public string TriggerName
		{
			get;
			internal set;
		}

		internal IDictionary<string, int> TriggerSubscriptions
		{
			get;
			set;
		}

		internal ITelemetryNotificationService NotificationService
		{
			get;
			set;
		}

		internal object TriggerLockObject
		{
			get;
			set;
		}

		/// <summary>
		/// Unsubscribes from the trigger event specified by TriggerName
		/// </summary>
		public void Unsubscribe()
		{
			if (NotificationService != null && TriggerSubscriptions != null)
			{
				lock (TriggerLockObject)
				{
					if (TriggerSubscriptions.ContainsKey(TriggerName))
					{
						NotificationService.Unsubscribe(TriggerSubscriptions[TriggerName]);
						TriggerSubscriptions.Remove(TriggerName);
					}
				}
			}
		}

		/// <summary>
		/// Unsubscribes from all triggers specified alongside TriggerName
		/// in the Targeted Notifications back-end.
		/// </summary>
		public void UnsubscribeAll()
		{
			if (NotificationService != null && TriggerSubscriptions != null)
			{
				lock (TriggerLockObject)
				{
					if (TriggerSubscriptions.Count > 0)
					{
						foreach (int value in TriggerSubscriptions.Values)
						{
							NotificationService.Unsubscribe(value);
						}
						TriggerSubscriptions.Clear();
					}
				}
			}
		}
	}
}
