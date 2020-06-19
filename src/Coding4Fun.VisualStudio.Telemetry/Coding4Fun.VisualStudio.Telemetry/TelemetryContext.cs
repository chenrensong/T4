using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Telemetry Context is a concept of a unit of work.
	/// More details <a href="http://devdiv/sites/vsplat/Fundamentals/Shared Documents/Telemetry/Projects/Telemetry API/TelemetrySession Speclet.docx?Web=1">here</a>
	/// </summary>
	public class TelemetryContext : TelemetryDisposableObject
	{
		private class PostPropertyEntry
		{
			public string Key
			{
				get;
			}

			public object Value
			{
				get;
			}

			public bool IsReserved
			{
				get;
			}

			public PostPropertyEntry(string key, object value, bool isReserved)
			{
				Key = key;
				Value = value;
				IsReserved = isReserved;
			}
		}

		private const int SchedulerDelay = 15;

		private const string ContextPropertyPrefix = "Context.";

		private const string ContextEventPrefix = "Context/";

		private const string ContextEventCreate = "Create";

		private const string ContextEventClose = "Close";

		private const string ContextEventPostProperty = "PostProperty";

		private readonly TelemetryPropertyBag sharedProperties = new TelemetryPropertyBag();

		private readonly TelemetryPropertyBags.Concurrent<Func<object>> realtimeSharedProperties = new TelemetryPropertyBags.Concurrent<Func<object>>();

		private readonly TelemetrySession hostSession;

		private readonly bool overrideInit;

		private readonly object disposeLocker = new object();

		private readonly ITelemetryScheduler scheduler;

		private readonly ConcurrentQueue<PostPropertyEntry> postedProperties = new ConcurrentQueue<PostPropertyEntry>();

		private readonly DateTime contextStart = DateTime.UtcNow;

		private bool disposedContextPart;

		/// <summary>
		/// Gets Shared properties that are added to each event until the context is closed.
		/// Shared properties have prefix "Context.%ContextName%."
		/// </summary>
		public IDictionary<string, object> SharedProperties => sharedProperties;

		/// <summary>
		/// Gets Realtime Shared properties calculated and added to each event until the context is closed.
		/// Shared properties have prefix "Context.%ContextName%."
		/// </summary>
		public IDictionary<string, Func<object>> RealtimeSharedProperties => realtimeSharedProperties;

		/// <summary>
		/// Gets a value indicating whether we have shared properties.
		/// This is implemented in order to avoid instantiation of empty SharedProperties dictionary
		/// </summary>
		public bool HasSharedProperties => sharedProperties.HasProperties();

		/// <summary>
		/// Gets ContextName which serves as convenient way to differ between properties from different contexts.
		/// ContextName added to the prefix of the shared properties.
		/// </summary>
		public string ContextName
		{
			get;
			private set;
		}

		/// <summary>
		/// Post regular context property.
		/// That property is posted to the backend immediately and not attached to the every event.
		/// You may want to consider <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySettingProperty" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryMetricProperty" />
		/// These will enable a richer telemetry experience with additional insights provided by Visual Studio Data Model.
		/// If you have any questions regarding VS Data Model, please email VS Data Model Crew (vsdmcrew@microsoft.com).
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		public void PostProperty(string propertyName, object propertyValue)
		{
			PostProperty(propertyName, propertyValue, false);
		}

		/// <summary>
		/// Post context property. That property is posted to the backend immediately and not attached to the every event.
		/// Property could be reserved or regular.
		/// Reserved property will accomplished with prefix "Reserved."
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		/// <param name="isReserved">is property reserved</param>
		internal void PostProperty(string propertyName, object propertyValue, bool isReserved)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(propertyName, "propertyName");
				CodeContract.RequiresArgumentNotNull<object>(propertyValue, "propertyValue");
				postedProperties.Enqueue(new PostPropertyEntry(propertyName, propertyValue, isReserved));
				Action action = FlushPostedProperties;
				scheduler.ScheduleTimed(action);
			}
		}

		internal void FlushPostedProperties()
		{
			RequiresNotDisposed();
			if (postedProperties.Count == 0 || !scheduler.CanEnterTimedDelegate())
			{
				return;
			}
			TelemetryEvent telemetryEvent = CreateTelemetryEvent("PostProperty");
			PostPropertyEntry result;
			while (postedProperties.TryDequeue(out result))
			{
				if (result.IsReserved)
				{
					telemetryEvent.ReservedProperties[result.Key] = result.Value;
				}
				else
				{
					telemetryEvent.Properties[result.Key] = result.Value;
				}
			}
			TelemetrySession.ValidateEvent(telemetryEvent);
			ValidateEventProperties(telemetryEvent);
			AddReservedPropertiesToTheEvent(telemetryEvent);
			hostSession.PostValidatedEvent(telemetryEvent);
			scheduler.ExitTimedDelegate();
		}

		/// <summary>
		/// Create TelemetrySessionContext with the name
		/// </summary>
		/// <param name="contextName"></param>
		/// <param name="theHostedSession">Session which owns this context</param>
		/// <param name="theScheduler"></param>
		/// <param name="theOverrideInit"></param>
		/// <param name="initializationAction"></param>
		internal TelemetryContext(string contextName, TelemetrySession theHostedSession, ITelemetryScheduler theScheduler = null, bool theOverrideInit = false, Action<TelemetryContext> initializationAction = null)
		{
			if (!IsContextNameValid(contextName))
			{
				throw new ArgumentException("contextName is invalid, contextName must contain alphanumeric characters only");
			}
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(theHostedSession, "theHostedSession");
			if (theScheduler == null)
			{
				theScheduler = new TelemetryScheduler();
				theScheduler.InitializeTimed(TimeSpan.FromSeconds(15.0));
			}
			ContextName = contextName;
			hostSession = theHostedSession;
			scheduler = theScheduler;
			overrideInit = theOverrideInit;
			hostSession.AddContext(this);
			initializationAction?.Invoke(this);
			if (!overrideInit)
			{
				hostSession.PostValidatedEvent(BuildStartEvent());
			}
		}

		/// <summary>
		/// Check whether context name is valid
		/// </summary>
		/// <param name="contextName"></param>
		/// <returns></returns>
		internal static bool IsContextNameValid(string contextName)
		{
			if (string.IsNullOrEmpty(contextName))
			{
				return false;
			}
			return contextName.All(char.IsLetterOrDigit);
		}

		/// <summary>
		/// Process telemetry event. Add all shared properties
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="overwriteExisting"></param>
		internal void ProcessEvent(TelemetryEvent telemetryEvent, bool overwriteExisting = true)
		{
			foreach (KeyValuePair<string, object> sharedProperty in SharedProperties)
			{
				string key = BuildPropertyName(sharedProperty.Key);
				if (overwriteExisting || !telemetryEvent.Properties.ContainsKey(key))
				{
					telemetryEvent.Properties[key] = sharedProperty.Value;
				}
			}
		}

		/// <summary>
		/// Add all real-time shared properties to event
		/// </summary>
		/// <param name="telemetryEvent"></param>
		internal void ProcessEventRealtime(TelemetryEvent telemetryEvent)
		{
			foreach (KeyValuePair<string, Func<object>> realtimeSharedProperty in realtimeSharedProperties)
			{
				object obj = realtimeSharedProperty.Value();
				if (obj != null)
				{
					string key = BuildPropertyName(realtimeSharedProperty.Key);
					telemetryEvent.Properties[key] = obj;
				}
			}
		}

		/// <summary>
		/// Context event validator. We have to check that properties doesn't contain reserved prefixes
		/// </summary>
		/// <param name="telemetryEvent"></param>
		internal static void ValidateEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			ValidateEventName(telemetryEvent);
			ValidateEventProperties(telemetryEvent);
		}

		/// <summary>
		/// Validate property name. Check whether property name prefix is not match to the reserved prefixes.
		/// </summary>
		/// <param name="propertyName"></param>
		internal static void ValidatePropertyName(string propertyName)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(propertyName, "propertyName");
			if (IsPropertyNameReserved(propertyName))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "property '{0}' has reserved prefix '{1}'", new object[2]
				{
					propertyName,
					"Context."
				}));
			}
		}

		/// <summary>
		/// Check whether property name is reserved
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		internal static bool IsPropertyNameReserved(string propertyName)
		{
			return propertyName.StartsWith("Context.", StringComparison.Ordinal);
		}

		/// <summary>
		/// Validate whether event name is context/postproperty
		/// </summary>
		/// <param name="eventName"></param>
		/// <returns></returns>
		internal static bool IsEventNameContextPostProperty(string eventName)
		{
			return eventName.Equals(BuildEventName("PostProperty"), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Dispose managed resources implementation
		/// </summary>
		protected override void DisposeManagedResources()
		{
			if (!disposedContextPart)
			{
				lock (disposeLocker)
				{
					if (!disposedContextPart)
					{
						scheduler.CancelTimed(true);
						FlushPostedProperties();
						if (!overrideInit)
						{
							hostSession.PostValidatedEvent(BuildCloseEvent());
						}
						hostSession.RemoveContext(this);
						disposedContextPart = true;
					}
				}
			}
		}

		/// <summary>
		/// Validate event name in terms of the context validation.
		/// Name is valid if it is not reserved.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		private static void ValidateEventName(TelemetryEvent telemetryEvent)
		{
			if (telemetryEvent.Name.StartsWith("Context/", StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "event '{0}' has reserved prefix '{1}'", new object[2]
				{
					telemetryEvent.Name,
					"Context/"
				}));
			}
		}

		/// <summary>
		/// Validate event properties in terms of the context validation.
		/// Property is valid if it is not contain reserved prefix.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		private static void ValidateEventProperties(TelemetryEvent telemetryEvent)
		{
			foreach (KeyValuePair<string, object> property in telemetryEvent.Properties)
			{
				ValidatePropertyName(property.Key);
			}
		}

		/// <summary>
		/// Build context event name
		/// </summary>
		/// <param name="eventName"></param>
		/// <returns></returns>
		private static string BuildEventName(string eventName)
		{
			return "Context/" + eventName;
		}

		private string BuildPropertyName(string propertyName)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}.{2}", new object[3]
			{
				"Context.",
				ContextName,
				propertyName
			});
		}

		/// <summary>
		/// Build context start event
		/// </summary>
		/// <returns></returns>
		private TelemetryEvent BuildStartEvent()
		{
			TelemetryEvent telemetryEvent = CreateTelemetryEvent("Create");
			AddReservedPropertiesToTheEvent(telemetryEvent);
			return telemetryEvent;
		}

		/// <summary>
		/// Build context end event
		/// </summary>
		/// <returns></returns>
		private TelemetryEvent BuildCloseEvent()
		{
			TelemetryEvent telemetryEvent = CreateTelemetryEvent("Close");
			AddReservedPropertiesToTheEvent(telemetryEvent);
			telemetryEvent.ReservedProperties["ContextDurationInMs"] = Math.Round(DateTime.UtcNow.Subtract(contextStart).TotalMilliseconds);
			return telemetryEvent;
		}

		/// <summary>
		/// Add reserved properties (Id and Name) to the event
		/// <a href="http://devdiv/sites/vsplat/Fundamentals/Shared Documents/Telemetry/Projects/Telemetry API/TelemetrySession Speclet.docx?Web=1" />
		/// </summary>
		/// <param name="telemetryEvent"></param>
		private void AddReservedPropertiesToTheEvent(TelemetryEvent telemetryEvent)
		{
			telemetryEvent.ReservedProperties["ContextName"] = ContextName;
		}

		/// <summary>
		/// Create a telemetry event
		/// </summary>
		/// <param name="eventName"></param>
		/// <returns></returns>
		private TelemetryEvent CreateTelemetryEvent(string eventName)
		{
			return new TelemetryEvent(BuildEventName(eventName));
		}
	}
}
