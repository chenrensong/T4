using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// [OBSOLETE]
	/// Please use data model <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> to track performance in dev15 and above releases.
	/// More details is at http://aka.ms/datamodel.
	/// </summary>
	public sealed class TelemetryActivity : TelemetryEvent
	{
		private readonly Guid correlationId;

		private readonly Guid parentCorrelationId;

		/// <summary>
		/// Gets activity correlation id for the current activity
		/// </summary>
		public Guid CorrelationId => correlationId;

		/// <summary>
		/// Gets correlation Id for this activity's parent activity
		/// </summary>
		internal Guid ParentCorrelationId => parentCorrelationId;

		/// <summary>
		/// Gets begin timestamp of the activity (in UTC)
		/// </summary>
		internal DateTime StartTime
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets end timestamp of the activity (in UTC)
		/// </summary>
		internal DateTime EndTime
		{
			get;
			private set;
		}

		/// <summary>
		/// Creates the new telemetry activity class.
		/// </summary>
		/// <param name="eventName">Event name that is unique, not null and not empty.</param>
		public TelemetryActivity(string eventName)
			: this(eventName, Guid.Empty)
		{
		}

		/// <summary>
		/// Creates a new telemetry activity parented to another activity
		/// </summary>
		/// <param name="eventName">Event name that is unique, not null and not empty.</param>
		/// <param name="parentCorrelationId">Correlation Id of the parent event</param>
		public TelemetryActivity(string eventName, Guid parentCorrelationId)
			: base(eventName)
		{
			correlationId = Guid.NewGuid();
			this.parentCorrelationId = parentCorrelationId;
			StartTime = DateTime.MinValue;
			EndTime = DateTime.MinValue;
		}

		/// <summary>
		/// Marks the activity as began and registers the current timestamp
		/// </summary>
		public void Start()
		{
			if (StartTime != DateTime.MinValue)
			{
				throw new InvalidOperationException("Activity is already started.");
			}
			StartTime = DateTime.UtcNow;
			TelemetryService.TelemetryEventSource.WriteActivityStartEvent(this);
		}

		/// <summary>
		/// Marks the activity as ended and registers the current timestamp
		/// </summary>
		public void End()
		{
			if (StartTime == DateTime.MinValue)
			{
				throw new InvalidOperationException("Activity is not yet started.");
			}
			if (EndTime != DateTime.MinValue)
			{
				throw new InvalidOperationException("Activity is already ended.");
			}
			EndTime = DateTime.UtcNow;
			TelemetryService.TelemetryEventSource.WriteActivityStopEvent(this);
		}

		/// <summary>
		/// Marks the activity as ended and sets the duration and start time per given the duration
		/// </summary>
		/// <param name="duration">Duration of the activity in milliseconds</param>
		public void End(TimeSpan duration)
		{
			if (StartTime != DateTime.MinValue)
			{
				throw new InvalidOperationException("Activity is already started and can not be ended with known duration.");
			}
			if (EndTime != DateTime.MinValue)
			{
				throw new InvalidOperationException("Activity is already ended.");
			}
			EndTime = DateTime.UtcNow;
			StartTime = EndTime - duration;
			TelemetryService.TelemetryEventSource.WriteActivityEndWithDurationEvent(this);
		}

		/// <summary>
		/// Returns default properties that should be on each TelemetryEvent
		/// </summary>
		/// <param name="eventTime">A time when the event happend</param>
		/// <param name="processStartTime">A time when the session started</param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		protected override IEnumerable<KeyValuePair<string, object>> GetDefaultEventProperties(long eventTime, long processStartTime, string sessionId)
		{
			foreach (KeyValuePair<string, object> defaultEventProperty in base.GetDefaultEventProperties(eventTime, processStartTime, sessionId))
			{
				yield return defaultEventProperty;
			}
			if (StartTime > DateTime.MinValue)
			{
				yield return new KeyValuePair<string, object>("Activity.StartTime", Math.Round(new TimeSpan(StartTime.Ticks - processStartTime).TotalMilliseconds));
			}
			else
			{
				yield return new KeyValuePair<string, object>("Activity.StartTime", -1);
			}
			if (EndTime > DateTime.MinValue)
			{
				yield return new KeyValuePair<string, object>("Activity.EndTime", Math.Round(new TimeSpan(EndTime.Ticks - processStartTime).TotalMilliseconds));
				yield return new KeyValuePair<string, object>("Activity.Duration", Math.Round(new TimeSpan(EndTime.Ticks - StartTime.Ticks).TotalMilliseconds));
			}
			else
			{
				yield return new KeyValuePair<string, object>("Activity.EndTime", -1);
				yield return new KeyValuePair<string, object>("Activity.Duration", -1);
			}
			yield return new KeyValuePair<string, object>("Activity.CorrelationId", CorrelationId);
			if (ParentCorrelationId != Guid.Empty)
			{
				yield return new KeyValuePair<string, object>("Activity.ParentCorrelationId", ParentCorrelationId);
			}
		}
	}
}
