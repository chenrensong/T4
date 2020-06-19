using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// The class represents a telemetry event that can be posted to a server.
	/// Class is NOT thread-safe
	/// </summary>
	public class TelemetryEvent
	{
		/// <summary>
		/// Please contact vsdmcrew@microsoft.com before making any change.
		/// Bump the version for any conditions below for data model events.
		/// 1. add new property.
		/// 2. remove property.
		/// 3. change the meaning of property value.
		/// 4. change the data type of property value.
		/// </summary>
		private const int SchemaVersion = 5;

		/// <summary>
		/// A string to indicate data source of telemetry event. It is used for backend server processing.
		/// </summary>
		private const string DataModelApiSource = "DataModelApi";

		private const TelemetrySeverity DefaultSeverity = TelemetrySeverity.Normal;

		internal const string ReservedPropertyPrefix = "Reserved.";

		private readonly string eventName;

		/// <summary>
		/// Event property key-value storage
		/// </summary>
		private readonly TelemetryPropertyBags.NotConcurrent<object> eventProperties = new TelemetryPropertyBags.NotConcurrent<object>();

		/// <summary>
		/// Reserved properties key-value storage
		/// </summary>
		private readonly TelemetryPropertyBags.NotConcurrent<object> reservedEventProperties = new TelemetryPropertyBags.NotConcurrent<object>();

		private readonly HashSet<TelemetryPropertyBag> sharedPropertyBags = new HashSet<TelemetryPropertyBag>();

		/// <summary>
		/// Each event should have its own unique id to be able to dedupe or match on a server side
		/// </summary>
		private Guid eventId;

		/// <summary>
		/// Severity level for this event.
		/// </summary>
		private TelemetrySeverity severity;

		internal Dictionary<TelemetryEventCorrelation, string> CorrelatedWith
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether event is friendly for the optOut session.
		/// By default it is false.
		/// If it is OptOut friendly it passes through with the event specific properties only.
		/// This behaviour can be changed by manifest rules.
		/// </summary>
		public bool IsOptOutFriendly
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a severity level of the event.
		/// The level is used for event consumer (e.g., ETW provider, backend reporting) to organize data easier.
		/// </summary>
		public TelemetrySeverity Severity
		{
			get
			{
				return severity;
			}
			set
			{
				severity = value;
				ReservedProperties["DataModel.Severity"] = (int)severity;
			}
		}

		/// <summary>
		/// Gets event type for this event
		/// </summary>
		public DataModelEventType EventType => Correlation.EventType;

		/// <summary>
		/// Gets schema version for this event.
		/// </summary>
		public int EventSchemaVersion
		{
			get;
		}

		/// <summary>
		/// Gets data source.
		/// </summary>
		public string DataSource => "DataModelApi";

		/// <summary>
		/// Gets correlation of this event. It represents this event when correlated with other events.
		/// </summary>
		public TelemetryEventCorrelation Correlation
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets current event name
		/// </summary>
		/// <returns></returns>
		public string Name => eventName;

		/// <summary>
		/// Gets a dictionary of event properties.
		/// Properties are dimensions that aggregated data can be sliced by.
		/// The key is a property name that is unique, not null and not empty.
		/// The value is any object that represents a property value.
		/// Telemetry channels must use value.ToString(CultureInfo.InvariantCulture)
		/// to send the value to a server as a string.
		/// </summary>
		public IDictionary<string, object> Properties => eventProperties;

		/// <summary>
		/// Gets a value indicating whether properties already created.
		/// </summary>
		public bool HasProperties => eventProperties.HasProperties();

		/// <summary>
		/// Gets or sets timestamp of the event when it is going to be posted
		/// Set by TelemetrySession
		/// </summary>
		internal DateTimeOffset PostTimestamp
		{
			get;
			set;
		}

		/// <summary>
		/// Gets shared property bags
		/// </summary>
		public HashSet<TelemetryPropertyBag> SharedPropertyBags => sharedPropertyBags;

		/// <summary>
		/// Gets a dictionary of reserved event properties.
		/// These properties are for internal purposes, such as
		/// event timestamps, activity attributes.
		/// </summary>
		internal IDictionary<string, object> ReservedProperties => reservedEventProperties;

		/// <summary>
		/// Gets a value indicating whether reserved properties already created.
		/// </summary>
		internal bool HasReservedProperties => reservedEventProperties.HasProperties();

		/// <summary>
		/// Creates the new telemetry event instance.
		/// You should consider choosing <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" />, <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />, <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />, <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" />,
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySettingProperty" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryMetricProperty" />
		/// These will enable a richer telemetry experience with additional insights provided by Visual Studio Data Model.
		/// If your data point doesn't align with any VS Data Model entity, please don't force any association and continue to use this method.
		/// If you have any questions regarding VS Data Model, please email VS Data Model Crew (vsdmcrew@microsoft.com).
		/// </summary>
		/// <param name="eventName">Event name that is unique, not null and not empty.</param>
		public TelemetryEvent(string eventName)
			: this(eventName, TelemetrySeverity.Normal)
		{
		}

		/// <summary>
		/// Creates the new telemetry event instance with severity information.
		/// You should consider choosing <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" />, <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />, <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />, <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" />,
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySettingProperty" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryMetricProperty" />
		/// These will enable a richer telemetry experience with additional insights provided by Visual Studio Data Model.
		/// If your data point doesn't align with any VS Data Model entity, please don't force any association and continue to use this method.
		/// If you have any questions regarding VS Data Model, please email VS Data Model Crew (vsdmcrew@microsoft.com).
		/// </summary>
		/// <param name="eventName">Event name that is unique, not null and not empty.</param>
		/// <param name="severity">Severity level of the event.</param>
		public TelemetryEvent(string eventName, TelemetrySeverity severity)
			: this(eventName, severity, DataModelEventType.Trace)
		{
		}

		/// <summary>
		/// Correlate this event with other events via <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation" />.
		/// </summary>
		/// <param name="correlations">An array of <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" /> that represents the correlated events.</param>
		/// <remarks>
		/// This method is not thread-safe.
		/// </remarks>
		public void Correlate(params TelemetryEventCorrelation[] correlations)
		{
			if (correlations != null)
			{
				foreach (TelemetryEventCorrelation correlation in correlations)
				{
					CorrelateWithDescription(correlation, null);
				}
			}
		}

		/// <summary>
		/// Creates the new telemetry event instance with specific information.
		/// </summary>
		/// <param name="eventName">Event name that is unique, not null and not empty.</param>
		/// <param name="severity">Severity level of the event.</param>
		/// <param name="eventType">Data Model type of this event. check <see cref="T:Coding4Fun.VisualStudio.Telemetry.DataModelEventType" /> for full type list. </param>
		internal TelemetryEvent(string eventName, TelemetrySeverity severity, DataModelEventType eventType)
			: this(eventName, severity, new TelemetryEventCorrelation(Guid.NewGuid(), eventType))
		{
		}

		/// <summary>
		/// Creates the new telemetry event instance with specific information.
		/// </summary>
		/// <param name="eventName">Event name that is unique, not null and not empty.</param>
		/// <param name="severity">Severity level of the event.</param>
		/// <param name="correlation">Correlation value for this event.</param>
		internal TelemetryEvent(string eventName, TelemetrySeverity severity, TelemetryEventCorrelation correlation)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(eventName, "eventName");
			correlation.RequireNotEmpty("correlation");
			TelemetryService.EnsureEtwProviderInitialized();
			this.eventName = eventName.ToLower(CultureInfo.InvariantCulture);
			Severity = severity;
			Correlation = correlation;
			EventSchemaVersion = 5;
			InitDataModelBasicProperties();
		}

		/// <summary>
		/// Creates an event for a channel from this event and session start time.
		/// Note: a returned event is not a pure copy of this event.
		/// </summary>
		/// <param name="processStartTime"></param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		internal TelemetryEvent BuildChannelEvent(long processStartTime, string sessionId)
		{
			eventId = Guid.NewGuid();
			TelemetryEvent telemetryEvent = new TelemetryEvent(eventName, Severity, EventType);
			telemetryEvent.IsOptOutFriendly = IsOptOutFriendly;
			telemetryEvent.Correlation = Correlation;
			foreach (KeyValuePair<string, object> allProperty in GetAllProperties(DateTime.UtcNow.Ticks, processStartTime, sessionId))
			{
				telemetryEvent.eventProperties[allProperty.Key] = allProperty.Value;
			}
			telemetryEvent.ReservedProperties.Clear();
			DictionaryExtensions.AddRange<string, object>(telemetryEvent.ReservedProperties, ReservedProperties, true);
			return telemetryEvent;
		}

		/// <summary>
		/// Don't want to expose an interface, like IClonable publicly, so we use an internal method that clones from a ChannelEvent
		/// </summary>
		/// <returns></returns>
		internal TelemetryEvent CloneTelemetryEvent()
		{
			TelemetryEvent obj = new TelemetryEvent(eventName, Severity, EventType)
			{
				eventId = eventId,
				IsOptOutFriendly = IsOptOutFriendly,
				Correlation = Correlation,
				PostTimestamp = PostTimestamp
			};
			DictionaryExtensions.AddRange<string, object>((IDictionary<string, object>)obj.eventProperties, (IDictionary<string, object>)eventProperties, true);
			obj.ReservedProperties.Clear();
			DictionaryExtensions.AddRange<string, object>(obj.ReservedProperties, ReservedProperties, true);
			return obj;
		}

		/// <summary>
		/// Correlate this event with other event via <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation" /> with description information.
		/// </summary>
		/// <param name="correlation">The property <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" /> of correlated event.</param>
		/// <param name="description">
		/// A description string for this correlation information, such as name, hint, tag, category.
		/// Please don't include comma which is a reserved char.
		/// It could be null or empty string.
		/// </param>
		/// <remarks>
		/// This method is not thread-safe.
		/// </remarks>
		protected void CorrelateWithDescription(TelemetryEventCorrelation correlation, string description)
		{
			if (description != null && description.Contains(','))
			{
				throw new ArgumentException("Comma is not allowed.", "description");
			}
			if (!correlation.IsEmpty && !Correlation.Equals(correlation))
			{
				if (CorrelatedWith == null)
				{
					CorrelatedWith = new Dictionary<TelemetryEventCorrelation, string>();
				}
				CorrelatedWith[correlation] = description;
			}
		}

		/// <summary>
		/// Check whether property name is reserved
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		internal static bool IsPropertyNameReserved(string propertyName)
		{
			return propertyName.StartsWith("Reserved.", StringComparison.Ordinal);
		}

		/// <summary>
		/// Returns default properties that should be on each TelemetryEvent
		/// </summary>
		/// <param name="eventTime">A time when the event happend</param>
		/// <param name="processStartTime">A time when the session started</param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		protected virtual IEnumerable<KeyValuePair<string, object>> GetDefaultEventProperties(long eventTime, long processStartTime, string sessionId)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(sessionId, "sessionId");
			yield return new KeyValuePair<string, object>("TimeSinceSessionStart", Math.Round(new TimeSpan(eventTime - processStartTime).TotalMilliseconds));
			yield return new KeyValuePair<string, object>("EventId", eventId);
			yield return new KeyValuePair<string, object>("SessionId", sessionId);
		}

		private static KeyValuePair<string, object> AsReservedProperty(KeyValuePair<string, object> property)
		{
			return new KeyValuePair<string, object>(string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[2]
			{
				"Reserved.",
				property.Key
			}), property.Value);
		}

		private IEnumerable<KeyValuePair<string, object>> GetCorrelatedWithProperties()
		{
			if (CorrelatedWith != null && CorrelatedWith.Any())
			{
				int index = 0;
				foreach (KeyValuePair<TelemetryEventCorrelation, string> item in CorrelatedWith)
				{
					index++;
					string value = StringExtensions.Join((IEnumerable<string>)new string[3]
					{
						item.Key.Id.ToString("D"),
						DataModelEventTypeNames.GetName(item.Key.EventType),
						item.Value ?? string.Empty
					}, ",");
					yield return new KeyValuePair<string, object>("DataModel.Correlation." + index.ToString(), value);
				}
			}
		}

		private IEnumerable<KeyValuePair<string, object>> GetAllProperties(long eventTime, long processStartTime, string sessionId)
		{
			IEnumerable<KeyValuePair<string, object>> second = GetDefaultEventProperties(eventTime, processStartTime, sessionId).Concat(reservedEventProperties).Concat(GetCorrelatedWithProperties()).Select(AsReservedProperty);
			return eventProperties.Concat(sharedPropertyBags.Where((TelemetryPropertyBag bag) => bag != null).SelectMany((TelemetryPropertyBag bag) => bag)).Concat(second);
		}

		/// <summary>
		/// Add data model common properties to telemetry events.
		/// </summary>
		private void InitDataModelBasicProperties()
		{
			ReservedProperties["DataModel.Source"] = "DataModelApi";
			ReservedProperties["DataModel.EntityType"] = DataModelEventTypeNames.GetName(EventType);
			ReservedProperties["DataModel.EntitySchemaVersion"] = EventSchemaVersion;
			ReservedProperties["DataModel.CorrelationId"] = Correlation.Id;
		}

		/// <summary>
		/// ToString to make debugging easier: show in debug watch window
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name ?? "";
		}
	}
}
