using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class is used to send data model events for an application work with duration and result.
	/// The event could be either <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />
	/// It sends one event at the beginning and the other one at the end of work.
	/// </summary>
	/// <typeparam name="T">An event being or inheriting from <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />, e.g., <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /></typeparam>
	public sealed class TelemetryScope<T> where T : OperationEvent
	{
		internal delegate T CreateNewEvent(OperationStageType stageType);

		private const int ScopeIsEnded = 1;

		private const int ScopeIsNotEnded = 0;

		private int isEnded;

		private TelemetrySession TelemetrySession
		{
			get;
			set;
		}

		private DateTime StartTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether the scope is end or not.
		/// </summary>
		public bool IsEnd => isEnded == 1;

		/// <summary>
		/// Gets an event that will be posted at the end of work.
		/// It is used to add extra properties for current work.
		/// Please don't post this event directly, use method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> instead.
		/// </summary>
		public T EndEvent
		{
			get;
		}

		/// <summary>
		/// Gets correlation of start event so user can correlate with this TelemetryScope.
		/// </summary>
		public TelemetryEventCorrelation Correlation => EndEvent.Correlation;

		/// <summary>
		/// Create and post an event for start point, and then create a user event for end point (but not posted.)
		/// </summary>
		/// <param name="telemetrySession">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="createNewEvent">A function to create a new event.</param>
		/// <param name="settings">A <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScopeSettings" /> object to control the TelemetryScope behavior.</param>
		/// <remarks>
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </remarks>
		internal TelemetryScope(TelemetrySession telemetrySession, string eventName, CreateNewEvent createNewEvent, TelemetryScopeSettings settings)
		{
			isEnded = 0;
			TelemetrySession = telemetrySession;
			Guid.NewGuid();
			StartTime = DateTime.UtcNow;
			T val = createNewEvent(OperationStageType.Start);
			val.Severity = settings.Severity;
			val.Correlate(settings.Correlations);
			val.IsOptOutFriendly = settings.IsOptOutFriendly;
			if (settings.StartEventProperties != null)
			{
				DictionaryExtensions.AddRange<string, object>(val.Properties, settings.StartEventProperties, true);
			}
			if (settings.PostStartEvent)
			{
				TelemetrySession.PostEvent(val);
			}
			EndEvent = val;
			EndEvent.SetPostStartEventProperty(settings.PostStartEvent);
			EndEvent.StageType = OperationStageType.End;
		}

		/// <summary>
		/// Marks the end of this work and post end event.
		/// </summary>
		/// <param name="result">the result of this user task. If the result is Failure, recommend correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.</param>
		/// <param name="resultSummary">
		/// a summary description for the result.
		/// it provides a little bit more details about the result without digging into it.
		/// when correlated with fault event, use this parameter to summarize the additional information stored in <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.
		/// E.g., "sign in failed because of wrong credential", "user cancelled azure deployment".
		/// Default value is null.
		/// </param>
		public void End(TelemetryResult result, string resultSummary = null)
		{
			if (Interlocked.CompareExchange(ref isEnded, 1, 0) == 1)
			{
				throw new InvalidOperationException("The scoped user task is already ended.");
			}
			EndEvent.SetResultProperties(result, resultSummary);
			EndEvent.SetTimeProperties(StartTime, DateTime.UtcNow, (DateTime.UtcNow - StartTime).TotalMilliseconds);
			TelemetrySession.PostEvent(EndEvent);
		}
	}
}
