using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.Notification
{
	/// <summary>
	/// Telemetry notification service allows subscribers to be notified when telemetry event matching a specified rule
	/// is posted.
	/// </summary>
	/// <remarks>
	/// Using a simple mutual-exclusion lock implementation and int as ID because heavy async consumptions of this
	/// service is not expected at this time.
	/// </remarks>
	public class TelemetryNotificationService : ITelemetryNotificationService, ISetTelemetrySession
	{
		private struct Subscription
		{
			public Action<TelemetryEvent> Handler;

			public ITelemetryEventMatch Match;

			public bool SingleNotification;
		}

		/// <summary>
		/// we can't have a readonly static lazy here because unit tests will just use the same instance, causing confusion and random errors, depending on the order of unit test execution
		/// </summary>
		private static Lazy<ITelemetryNotificationService> defaultLazy;

		private const string TelemetryNotificationBaseEventName = "VS/Core/TelemetryNotification";

		internal const string TelemetryNotificationFilterFaultEventName = "VS/Core/TelemetryNotification/FilterFault";

		internal const string TelemetryNotificationHandlerFaultEventName = "VS/Core/TelemetryNotification/HandlerFault";

		private readonly object lockObject = new object();

		private readonly Lazy<IDictionary<int, Subscription>> subscriptionsLazy = new Lazy<IDictionary<int, Subscription>>(() => new Dictionary<int, Subscription>());

		private ITelemetryNotificationProvider provider;

		private ITelemetryTestChannel channel;

		private int lastSubscriptionId;

		private ConcurrentQueue<TelemetryEvent> queueTelemetryEvents = new ConcurrentQueue<TelemetryEvent>();

		internal AsyncManualResetEvent EventNewItemAvailableForNotification = new AsyncManualResetEvent();

		private CancellationTokenSource cancellationTokenSource;

		/// <summary>
		/// Gets the default singleton instance of the telemetry notifications service.
		/// </summary>
		public static ITelemetryNotificationService Default => defaultLazy.Value;

		private IDictionary<int, Subscription> Subscriptions => subscriptionsLazy.Value;

		static TelemetryNotificationService()
		{
			Initialize();
		}

		/// <summary>
		/// initialize the defaultLazy. Pass in Null as TelemetrySession to use which means use TelemetryService.DefaultSession
		/// </summary>
		internal static void Initialize()
		{
			defaultLazy = new Lazy<ITelemetryNotificationService>(() => new TelemetryNotificationService(new TelemetryNotificationProvider(null)));
		}

		/// <summary>
		/// Internal constructor for unit testing.
		/// </summary>
		internal TelemetryNotificationService(ITelemetryNotificationProvider provider)
		{
			CodeContract.RequiresArgumentNotNull<ITelemetryNotificationProvider>(provider, "provider");
			this.provider = provider;
		}

		/// <inheritdoc />
		public int Subscribe(ITelemetryEventMatch eventMatch, Action<TelemetryEvent> handler, bool singleNotification = true)
		{
			CodeContract.RequiresArgumentNotNull<ITelemetryEventMatch>(eventMatch, "eventMatch");
			CodeContract.RequiresArgumentNotNull<Action<TelemetryEvent>>(handler, "handler");
			Subscription subscription = default(Subscription);
			subscription.Handler = handler;
			subscription.Match = eventMatch;
			subscription.SingleNotification = singleNotification;
			Subscription value = subscription;
			lock (lockObject)
			{
				lastSubscriptionId++;
				AttachChannel();
				Subscriptions[lastSubscriptionId] = value;
				return lastSubscriptionId;
			}
		}

		/// <inheritdoc />
		public void Unsubscribe(int subscriptionId)
		{
			lock (lockObject)
			{
				if (Subscriptions.Remove(subscriptionId) && Subscriptions.Count == 0)
				{
					DetachChannel();
				}
			}
		}

		private void AttachChannel()
		{
			lock (lockObject)
			{
				if (channel == null)
				{
					channel = new NotificationTelemetryChannel(OnPostEvent);
					provider.AttachChannel(channel);
					if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
					{
						cancellationTokenSource = new CancellationTokenSource();
						Task.Run(async delegate
						{
							await ListenForEventsInQueue();
						});
					}
				}
			}
		}

		private void DetachChannel()
		{
			lock (lockObject)
			{
				if (channel != null)
				{
					provider.DetachChannel(channel);
					channel = null;
					cancellationTokenSource.Cancel();
					EventNewItemAvailableForNotification.Set();
				}
			}
		}

		private async Task ListenForEventsInQueue()
		{
			while (!cancellationTokenSource.IsCancellationRequested)
			{
				await EventNewItemAvailableForNotification.WaitAsync();
				EventNewItemAvailableForNotification.Reset();
				while (queueTelemetryEvents.Count() > 0)
				{
					TelemetryEvent result = null;
					if (queueTelemetryEvents.TryDequeue(out result))
					{
						ProcessPostedEvents(result);
					}
				}
			}
		}

		/// <summary>
		/// We want to be very fast to post an event on the client thread, so we just add it to a queue, set an event, and return
		/// </summary>
		/// <param name="telemetryEvent"></param>
		private void OnPostEvent(TelemetryEvent telemetryEvent)
		{
			queueTelemetryEvents.Enqueue(telemetryEvent);
			EventNewItemAvailableForNotification.Set();
		}

		private void ProcessPostedEvents(TelemetryEvent telemetryEvent)
		{
			KeyValuePair<int, Subscription>[] array;
			lock (lockObject)
			{
				array = Subscriptions.ToArray();
			}
			KeyValuePair<int, Subscription>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<int, Subscription> keyValuePair = array2[i];
				Subscription value = keyValuePair.Value;
				bool flag = false;
				try
				{
					flag = value.Match.IsEventMatch(telemetryEvent);
				}
				catch (Exception exception)
				{
					PostFaultEvent("VS/Core/TelemetryNotification/FilterFault", value.Match, exception);
					Unsubscribe(keyValuePair.Key);
				}
				if (flag)
				{
					if (value.SingleNotification)
					{
						Unsubscribe(keyValuePair.Key);
					}
					try
					{
						TelemetryEvent obj = telemetryEvent.CloneTelemetryEvent();
						value.Handler(obj);
					}
					catch (Exception exception2)
					{
						PostFaultEvent("VS/Core/TelemetryNotification/HandlerFault", value.Match, exception2);
					}
				}
			}
		}

		private void PostFaultEvent(string eventName, ITelemetryEventMatch eventMatch, Exception exception)
		{
			string description;
			try
			{
				description = JsonConvert.SerializeObject((object)eventMatch, (Formatting)0);
			}
			catch (Exception ex)
			{
				description = ex.Message;
			}
			provider.PostFaultEvent(eventName, description, exception);
		}

		/// <inheritdoc />
		public void SetSession(TelemetrySession session)
		{
			lock (lockObject)
			{
				if (Subscriptions.Count > 0)
				{
					throw new InvalidOperationException("Cannot set the session after subscriptions have been made");
				}
				provider = new TelemetryNotificationProvider(session);
			}
		}
	}
}
