using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class to contain all data model extension methods to existing class TelemetrySession.
	/// </summary>
	public static class TelemetrySessionExtensions
	{
		/// <summary>
		/// Post an event for user task.
		/// A user task is an application operation that is INVOKED BY USER directly and comes with result (e.g., Success, Failure).
		/// It is used for user behavior/intent analysis. User is aware of the operation and be able to execute.
		/// e.g. Open project and Show tool windows are user tasks; instead load VS package and Design time build are operations.
		///
		/// This method is used for atomic user task that runs very fast or has little value to analyze the process duration. Caller calls this method when user task is complete.
		/// For long-time running or async user task, in order to understand what else happened during the time or track if it partially completes because of an error,
		/// use method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetrySessionExtensions.StartUserTask(Coding4Fun.VisualStudio.Telemetry.TelemetrySession,System.String)" /> which tracks both start and end points.
		/// </summary>
		/// <param name="session">telemetry session object.</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="result">the result of this user task. If the result is Failure, recommend correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.</param>
		/// <param name="resultSummary">
		/// a summary description for the result.
		/// it provides a little bit more details about the result without digging into it.
		/// when correlated with fault event, use this parameter to summarize the additional information stored in <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.
		/// E.g., "sign in failed because of wrong credential", "user cancelled azure deployment".
		/// Default value is null.
		/// </param>
		/// <param name="correlatedWith">
		/// Specify which events to correlate by using property <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" />
		/// Good candidates to correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> are,
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" />
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />
		/// </param>
		/// <returns>The user task event correlation.</returns>
		public static TelemetryEventCorrelation PostUserTask(this TelemetrySession session, string eventName, TelemetryResult result, string resultSummary = null, TelemetryEventCorrelation[] correlatedWith = null)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(session, "session");
			return session.PostOperationHelper(() => new UserTaskEvent(eventName, result, resultSummary), correlatedWith);
		}

		/// <summary>
		/// Post an Operation event.
		/// An operation performs some work in application and comes with result (e.g., Success, Failure).
		/// If the operation is invoked by user directly, please use <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or related methods.
		/// A few examples of operations are, license check, package load, windows layout loading.
		///
		/// This method is used for atomic operation that runs very fast or has little value to analyze the process duration. Caller calls this method when operation is complete.
		/// For long-time running or async operation, in order to understand what else happened during the time or track if it partially completes because of an error,
		/// use method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetrySessionExtensions.StartOperation(Coding4Fun.VisualStudio.Telemetry.TelemetrySession,System.String)" /> which tracks both start and end points.
		/// </summary>
		/// <param name="session">telemetry session object.</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="result">the result of this user task. If the result is Failure, recommend correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.</param>
		/// <param name="resultSummary">
		/// optional parameter. a summary description for the result.
		/// it provides a little bit more details about the result without digging into it.
		/// when correlated with fault event, use this parameter to summarize the additional information stored in <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.
		/// E.g., "sign in failed because of wrong credential", "user cancelled azure deployment".
		/// Default value is null.
		/// </param>
		/// <param name="correlatedWith">
		/// Specify which events to correlate by using property <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" />
		/// Good candidates to correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> are,
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" />
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" />
		/// </param>
		/// <returns>The operation event correlation.</returns>
		public static TelemetryEventCorrelation PostOperation(this TelemetrySession session, string eventName, TelemetryResult result, string resultSummary = null, TelemetryEventCorrelation[] correlatedWith = null)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(session, "session");
			return session.PostOperationHelper(() => new OperationEvent(eventName, result, resultSummary), correlatedWith);
		}

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<UserTaskEvent> StartUserTask(this TelemetrySession session, string eventName)
		{
			return session.StartUserTask(eventName, TelemetrySeverity.High, null, null);
		}

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="severity">
		/// A severity level of the event.
		/// The level is used for event consumer (e.g., ETW provider, backend reporting) to organize data easier.
		/// </param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<UserTaskEvent> StartUserTask(this TelemetrySession session, string eventName, TelemetrySeverity severity)
		{
			return session.StartUserTask(eventName, severity, null, null);
		}

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> with specified properties at the beginning of user task work,
		/// and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="severity">
		/// A severity level of the event.
		/// The level is used for event consumer (e.g., ETW provider, backend reporting) to organize data easier.
		/// </param>
		/// <param name="startEventProperties">
		/// Event properties for the start event of this scope. They are also copied to end event.
		/// </param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<UserTaskEvent> StartUserTask(this TelemetrySession session, string eventName, TelemetrySeverity severity, IDictionary<string, object> startEventProperties)
		{
			return session.StartUserTask(eventName, severity, startEventProperties, null);
		}

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> with specified properties at the beginning of user task work,
		/// and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="severity">
		/// A severity level of the event.
		/// The level is used for event consumer (e.g., ETW provider, backend reporting) to organize data easier.
		/// </param>
		/// <param name="startEventProperties">
		/// Event properties for the start event of this scope. They are also copied to end event.
		/// </param>
		/// <param name="correlations">Events with which this scope can correlate.</param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<UserTaskEvent> StartUserTask(this TelemetrySession session, string eventName, TelemetrySeverity severity, IDictionary<string, object> startEventProperties, TelemetryEventCorrelation[] correlations)
		{
			TelemetryScopeSettings settings = new TelemetryScopeSettings
			{
				Severity = severity,
				StartEventProperties = startEventProperties,
				Correlations = correlations
			};
			return session.StartUserTask(eventName, settings);
		}

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> with specified properties at the beginning of user task work,
		/// and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="settings">A <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScopeSettings" /> object to control the TelemetryScope behavior.</param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<UserTaskEvent> StartUserTask(this TelemetrySession session, string eventName, TelemetryScopeSettings settings)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(session, "session");
			CodeContract.RequiresArgumentNotNull<TelemetryScopeSettings>(settings, "settings");
			return new TelemetryScope<UserTaskEvent>(session, eventName, (OperationStageType stageType) => new UserTaskEvent(eventName, stageType, TelemetryResult.None), settings);
		}

		/// <summary>
		/// Start tracking operation by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the begining of operation work, and return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<OperationEvent> StartOperation(this TelemetrySession session, string eventName)
		{
			return session.StartOperation(eventName, TelemetrySeverity.Normal, null, null);
		}

		/// <summary>
		/// Start tracking operation by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the begining of operation work, and return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="severity">
		/// A severity level of the event.
		/// The level is used for event consumer (e.g., ETW provider, backend reporting) to organize data easier.
		/// </param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<OperationEvent> StartOperation(this TelemetrySession session, string eventName, TelemetrySeverity severity)
		{
			return session.StartOperation(eventName, severity, null, null);
		}

		/// <summary>
		/// Start tracking operation by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> with specified properties at the begining of operation work,
		/// and return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="severity">
		/// A severity level of the event.
		/// The level is used for event consumer (e.g., ETW provider, backend reporting) to organize data easier.
		/// </param>
		/// <param name="startEventProperties">
		/// Event properties for the start event of this scope. They are also copied to end event.
		/// </param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<OperationEvent> StartOperation(this TelemetrySession session, string eventName, TelemetrySeverity severity, IDictionary<string, object> startEventProperties)
		{
			return session.StartOperation(eventName, severity, startEventProperties, null);
		}

		/// <summary>
		/// Start tracking operation by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> with specified properties at the begining of operation work,
		/// and return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="severity">
		/// A severity level of the event.
		/// The level is used for event consumer (e.g., ETW provider, backend reporting) to organize data easier.
		/// </param>
		/// <param name="startEventProperties">
		/// Event properties for the start event of this scope. They are also copied to end event.
		/// </param>
		/// <param name="correlations">Events with which this scope can correlate.</param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<OperationEvent> StartOperation(this TelemetrySession session, string eventName, TelemetrySeverity severity, IDictionary<string, object> startEventProperties, TelemetryEventCorrelation[] correlations)
		{
			TelemetryScopeSettings settings = new TelemetryScopeSettings
			{
				Severity = severity,
				StartEventProperties = startEventProperties,
				Correlations = correlations
			};
			return session.StartOperation(eventName, settings);
		}

		/// <summary>
		/// Start tracking operation by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> with specified properties at the begining of operation work,
		/// and return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1.End(Coding4Fun.VisualStudio.Telemetry.TelemetryResult,System.String)" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="session">Telemetry Session</param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="settings">A <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScopeSettings" /> object to control the TelemetryScope behavior.</param>
		/// <returns><see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> instance.</returns>
		public static TelemetryScope<OperationEvent> StartOperation(this TelemetrySession session, string eventName, TelemetryScopeSettings settings)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(session, "session");
			CodeContract.RequiresArgumentNotNull<TelemetryScopeSettings>(settings, "settings");
			return new TelemetryScope<OperationEvent>(session, eventName, (OperationStageType stageType) => new OperationEvent(eventName, stageType, TelemetryResult.None), settings);
		}

		/// <summary>
		/// Post a Fault event. The event will always be sent to AppInsights, but if it passes sampling,
		/// it gets posted to Wason as well.
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description">The desription is not put in a bucket parameter, but it is in the ErrorInformation.txt file in the
		/// Cab file sent to Watson, and in the AI event</param>
		/// <returns>The fault event correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description)
		{
			return telemetrySession.PostFault(eventName, description, FaultSeverity.Uncategorized);
		}

		/// <summary>
		/// Post a Fault event. The event will always be sent to AppInsights, but if it passes sampling,
		/// it gets posted to Wason as well.
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description">The desription is not put in a bucket parameter, but it is in the ErrorInformation.txt file in the
		/// Cab file sent to Watson, and in the AI event</param>
		/// <param name="faultSeverity">The severity of the fault, used to identify actionable or important faults in divisional tools and reporting.</param>
		/// <returns>The fault event correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description, FaultSeverity faultSeverity)
		{
			return telemetrySession.PostFault(eventName, description, faultSeverity, null, null);
		}

		/// <summary>
		/// Post a Fault Event with a managed Exception object. The bucket parameters are created from the exception object.
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="exceptionObject"></param>
		/// <returns>The fault event correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description, Exception exceptionObject)
		{
			return telemetrySession.PostFault(eventName, description, FaultSeverity.Uncategorized, exceptionObject);
		}

		/// <summary>
		/// Post a Fault Event with a managed Exception object. The bucket parameters are created from the exception object.
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="faultSeverity">The severity of the fault, used to identify actionable or important faults in divisional tools and reporting.</param>
		/// <param name="exceptionObject"></param>
		/// <returns>The fault event correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description, FaultSeverity faultSeverity, Exception exceptionObject)
		{
			return telemetrySession.PostFault(eventName, description, faultSeverity, exceptionObject, null);
		}

		/// <summary>
		/// Post a fault event with an exception object and a callback. The callback can be used to calculate expensive data to be sent
		/// to the Watson back end, such as JScript callstacks, etc
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="exceptionObject">can be null</param>
		/// <param name="gatherEventDetails">Allows the user to provide code to execute synchronously to gather computationally expensive info about the event</param>
		/// <returns>The fault correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description, Exception exceptionObject, Func<IFaultUtility, int> gatherEventDetails)
		{
			return telemetrySession.PostFault(eventName, description, FaultSeverity.Uncategorized, exceptionObject, gatherEventDetails);
		}

		/// <summary>
		/// Post a fault event with an exception object and a callback. The callback can be used to calculate expensive data to be sent
		/// to the Watson back end, such as JScript callstacks, etc
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="faultSeverity">The severity of the fault, used to identify actionable or important faults in divisional tools and reporting.</param>
		/// <param name="exceptionObject">can be null</param>
		/// <param name="gatherEventDetails">Allows the user to provide code to execute synchronously to gather computationally expensive info about the event</param>
		/// <returns>The fault correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description, FaultSeverity faultSeverity, Exception exceptionObject, Func<IFaultUtility, int> gatherEventDetails)
		{
			return telemetrySession.PostFault(eventName, description, faultSeverity, exceptionObject, gatherEventDetails, null);
		}

		/// <summary>
		/// Post a fault event with an exception object and a callback. The callback can be used to calculate expensive data to be sent
		/// to the Watson back end, such as JScript callstacks, etc
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="exceptionObject">can be null</param>
		/// <param name="gatherEventDetails">Allows the user to provide code to execute synchronously to gather computationally expensive info about the event</param>
		/// <param name="correlatedWith">
		/// Specify which events to correlate by using property <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" />
		/// Good candidates to correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" /> are,
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" />
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />
		/// </param>
		/// <returns>The fault correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description, Exception exceptionObject, Func<IFaultUtility, int> gatherEventDetails, TelemetryEventCorrelation[] correlatedWith)
		{
			return telemetrySession.PostFault(eventName, description, FaultSeverity.Uncategorized, exceptionObject, gatherEventDetails, correlatedWith);
		}

		/// <summary>
		/// Post a fault event with an exception object and a callback. The callback can be used to calculate expensive data to be sent
		/// to the Watson back end, such as JScript callstacks, etc
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="faultSeverity">The severity of the fault, used to identify actionable or important faults in divisional tools and reporting.</param>
		/// <param name="exceptionObject">can be null</param>
		/// <param name="gatherEventDetails">Allows the user to provide code to execute synchronously to gather computationally expensive info about the event</param>
		/// <param name="correlatedWith">
		/// Specify which events to correlate by using property <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" />
		/// Good candidates to correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" /> are,
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" />
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />
		/// </param>
		/// <returns>The fault correlation.</returns>
		public static TelemetryEventCorrelation PostFault(this TelemetrySession telemetrySession, string eventName, string description, FaultSeverity faultSeverity, Exception exceptionObject, Func<IFaultUtility, int> gatherEventDetails, TelemetryEventCorrelation[] correlatedWith)
		{
			FaultEvent faultEvent = new FaultEvent(eventName, description, faultSeverity, exceptionObject, gatherEventDetails);
			faultEvent.Correlate(correlatedWith);
			telemetrySession.PostEvent(faultEvent);
			return faultEvent.Correlation;
		}

		/// <summary>
		/// Post an Asset event.
		/// Asset is the target of user task or operation, e.g., Solution, Project, File, Extension, License, Designer.
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
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <param name="assetEventVersion">
		/// Used for customized properties versioning.
		/// E.g., project asset posts event with name "vs/platform/project".
		/// If the event is updated, uses this parameter to increment the version.
		/// </param>
		/// <param name="properties">customized properties for this asset event.</param>
		/// <param name="correlatedWith">
		/// Specify which events to correlate by using property <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" />
		/// Good candidates to correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" /> are,
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" /> (to build up asset hierarchy/extension.)
		/// </param>
		/// <returns>The asset event correlation.</returns>
		public static TelemetryEventCorrelation PostAsset(this TelemetrySession telemetrySession, string eventName, string assetId, int assetEventVersion, IDictionary<string, object> properties, TelemetryEventCorrelation[] correlatedWith = null)
		{
			CodeContract.RequiresArgumentNotNull<IDictionary<string, object>>(properties, "properties");
			AssetEvent assetEvent = new AssetEvent(eventName, assetId, assetEventVersion);
			DictionaryExtensions.AddRange<string, object>(assetEvent.Properties, properties, true);
			assetEvent.Correlate(correlatedWith);
			telemetrySession.PostEvent(assetEvent);
			return assetEvent.Correlation;
		}

		private static TelemetryEventCorrelation PostOperationHelper<T>(this TelemetrySession session, Func<T> createEvent, TelemetryEventCorrelation[] correlatedWith) where T : OperationEvent
		{
			T val = createEvent();
			val.Correlate(correlatedWith);
			session.PostEvent(val);
			return val.Correlation;
		}
	}
}
