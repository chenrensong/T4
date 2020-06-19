using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
    /// <summary>
    /// Default ETW implementation for telemetry service.
    /// </summary>
    internal sealed class TelemetryEtwProvider : ITelemetryEtwProvider
	{
		/// <summary>
		/// Event keywords for telemetry events. These are used to separate events related to each section of
		/// telemetry API
		/// </summary>
		internal static class TelemetryKeywords
		{
			/// <summary>
			/// Events related to service initialization, service wide messages
			/// </summary>
			public const EventKeywords Service = (EventKeywords)1L;

			/// <summary>
			/// Events related to telemetry session initialization, session wide messages
			/// </summary>
			public const EventKeywords Session = (EventKeywords)2L;

			/// <summary>
			/// Events related to telemetry event instances like posting events
			/// </summary>
			public const EventKeywords Event = (EventKeywords)4L;

			/// <summary>
			/// Events related to telemetry activity instances like start, stop, post events
			/// </summary>
			public const EventKeywords Activity = (EventKeywords)8L;

			/// <summary>
			/// Events related to telemetry context initilization, start, stop
			/// </summary>
			public const EventKeywords Context = (EventKeywords)16L;
		}

		/// <summary>
		/// Event data submitted for all telemetry event, contains basic session information
		/// </summary>
		[EventData]
		internal struct TelemetryEventData
		{
			[EventField]
			public string SessionId
			{
				get;
			}

			[EventField]
			public string HostName
			{
				get;
			}

			public TelemetryEventData(TelemetrySession session)
			{
				SessionId = ((session != null) ? session.SessionId : string.Empty);
				HostName = ((session != null) ? session.HostName : string.Empty);
			}
		}

		/// <summary>
		/// Event data submitted with telemetry activity instances that were ended with specified duration
		/// </summary>
		[EventData]
		internal struct TelemetryActivityDataWithDuration
		{
			[EventField]
			public string SessionId
			{
				get;
			}

			[EventField]
			public string HostName
			{
				get;
			}

			[EventField]
			public double Duration
			{
				get;
			}

			public TelemetryActivityDataWithDuration(TelemetrySession session, TelemetryActivity activity)
			{
				SessionId = ((session != null) ? session.SessionId : string.Empty);
				HostName = ((session != null) ? session.HostName : string.Empty);
				Duration = (activity.EndTime - activity.StartTime).TotalMilliseconds;
			}
		}

		/// <summary>
		/// Verbose telemetry event/activity data, used to pass properties to ETW stream
		/// </summary>
		[EventData]
		internal struct TelemetryEventDataVerbose
		{
			[EventField]
			public string SessionId
			{
				get;
			}

			[EventField]
			public string HostName
			{
				get;
			}

			[EventField]
			public IEnumerable<KeyValuePair<string, string>> Properties
			{
				get;
			}

			public TelemetryEventDataVerbose(TelemetrySession session, TelemetryEvent telemetryEvent)
			{
				SessionId = ((session != null) ? session.SessionId : string.Empty);
				HostName = ((session != null) ? session.HostName : string.Empty);
				if (telemetryEvent.HasProperties)
				{
					Properties = telemetryEvent.Properties.Select((KeyValuePair<string, object> kvp) => new KeyValuePair<string, string>(kvp.Key, Convert.ToString(kvp.Value, CultureInfo.InvariantCulture)));
				}
				else
				{
					Properties = null;
				}
			}
		}

		private readonly EventSource telemetryEventSource;

		/// <summary>
		/// Creates a new telemetry ETW provider
		/// </summary>
		/// <param name="providerName">Name of the provider, must be unique across the current AppDomain</param>
		public TelemetryEtwProvider(string providerName)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			telemetryEventSource = (EventSource)(object)new EventSource(providerName, (EventSourceSettings)8);
		}

		/// <summary>
		/// Writes start event for a TelemetryActivity
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		public void WriteActivityStartEvent(TelemetryActivity activity)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			EventSourceOptions val = default(EventSourceOptions);
			((val)).Opcode=((EventOpcode)1);
			((val)).Keywords=((EventKeywords)8);
			((val)).Level=((EventLevel)4);
			EventSourceOptions options = val;
			WriteTelemetryEventSimple(activity, options);
		}

		/// <summary>
		/// Writes stop event for a TelemetryActivity
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		public void WriteActivityStopEvent(TelemetryActivity activity)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			EventSourceOptions val = default(EventSourceOptions);
			val.Opcode=((EventOpcode)2);
			val.Keywords=((EventKeywords)8);
			val.Level=((EventLevel)4);
			EventSourceOptions options = val;
			WriteTelemetryEventSimple(activity, options);
			WriteTelemetryEventExtended(activity);
		}

		/// <summary>
		/// Writes event for a TelemetryActivity that was ended with a specified duration
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		public void WriteActivityEndWithDurationEvent(TelemetryActivity activity)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			EventSourceOptions val = default(EventSourceOptions);
			val.Opcode=((EventOpcode)0);
			val.Keywords=((EventKeywords)8);
			val.Level=((EventLevel)4);
			EventSourceOptions options = val;
			WriteEventWithActivityId<TelemetryActivityDataWithDuration>(userData: new TelemetryActivityDataWithDuration(null, activity), telemetryEvent: activity, eventName: activity.Name, options: options);
			WriteTelemetryEventExtended(activity);
		}

		/// <summary>
		/// Writes an event for a TelemetryActivity when it is posted to a session.
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		/// <param name="session"></param>
		public void WriteActivityPostEvent(TelemetryActivity activity, TelemetrySession session)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			EventSourceOptions val = default(EventSourceOptions);
			val.Keywords=((EventKeywords)8);
			val.Level=((EventLevel)5);
			EventSourceOptions options = val;
			WriteEventWithActivityId<TelemetryEventData>(userData: new TelemetryEventData(session), telemetryEvent: activity, eventName: activity.Name + "/Posted", options: options);
		}

		/// <summary>
		/// Writes an event to indicate a telemetry event being posted to a session
		/// </summary>
		/// <param name="telemetryEvent">Telemetry event instance</param>
		/// <param name="session"></param>
		public void WriteTelemetryPostEvent(TelemetryEvent telemetryEvent, TelemetrySession session)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryEvent is TelemetryActivity)
			{
				throw new ArgumentException("Telemetry Activity instances should call WriteActivityPostEvent", "telemetryEvent");
			}
			EventSourceOptions val = default(EventSourceOptions);
			val.Opcode=((EventOpcode)0);
			val.Keywords=((EventKeywords)4);
			val.Level=((EventLevel)4);
			EventSourceOptions options = val;
			WriteTelemetryEventSimple(telemetryEvent, options, session);
			WriteTelemetryEventExtended(telemetryEvent, session);
		}

		private void WriteTelemetryEventSimple(TelemetryEvent telemetryEvent, EventSourceOptions options, TelemetrySession session = null)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryEventSource.IsEnabled(((EventSourceOptions)(options)).Level, ((EventSourceOptions)(options)).Keywords))
			{
				TelemetryEventData userData = new TelemetryEventData(session);
				string name = telemetryEvent.Name;
				WriteEventWithActivityId(telemetryEvent, name, options, userData);
			}
		}

		private void WriteTelemetryEventExtended(TelemetryEvent telemetryEvent, TelemetrySession session = null)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			EventKeywords val = (EventKeywords)((telemetryEvent is TelemetryActivity) ? 8 : 4);
			if (telemetryEventSource.IsEnabled((EventLevel)5, val))
			{
				EventSourceOptions val2 = default(EventSourceOptions);
				val2.Opcode=((EventOpcode)0);
				val2.Keywords=(val);
				val2.Level=((EventLevel)5);
				EventSourceOptions options = val2;
				TelemetryEventDataVerbose userData = new TelemetryEventDataVerbose(session, telemetryEvent);
				string eventName = telemetryEvent.Name + "/Verbose";
				WriteEventWithActivityId(telemetryEvent, eventName, options, userData);
			}
		}

		private void WriteEventWithActivityId<T>(TelemetryEvent telemetryEvent, string eventName, EventSourceOptions options, T userData)
		{
			Guid guid = (telemetryEvent as TelemetryActivity)?.CorrelationId ?? Guid.Empty;
			Guid empty = Guid.Empty;
			telemetryEventSource.Write<T>(eventName, ref options, ref guid, ref empty, ref userData);
		}
	}
}
