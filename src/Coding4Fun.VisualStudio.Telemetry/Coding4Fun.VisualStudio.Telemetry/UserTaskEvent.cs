namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class that stores information for user task data model event.
	/// A user task is an application operation that is INVOKED BY USER directly and comes with result (e.g., Success, Failure).
	/// It is used for user behavior/intent analysis. User is aware of the operation and be able to execute.
	/// e.g. Open project and Show tool windows are user tasks; instead load VS package and Design time build are operations.
	///
	/// For long-time running or async user task, in order to understand what else happened during the time or track if it partially completes because of an error,
	/// use method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetrySessionExtensions.StartUserTask(Coding4Fun.VisualStudio.Telemetry.TelemetrySession,System.String)" /> which tracks both start and end points.
	/// </summary>
	public sealed class UserTaskEvent : OperationEvent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> class.
		/// </summary>
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
		/// <example>
		/// This example shows how to create and post <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" />.
		/// <code>
		///     UserTask userTask = new UserTask("vs/debugger/stepinto", Result.Success);
		///     TelemetryService.DefaultSession.PostEvent(userTask);
		/// </code>
		/// </example>
		public UserTaskEvent(string eventName, TelemetryResult result, string resultSummary = null)
			: this(eventName, OperationStageType.Atomic, result, resultSummary)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> class.
		/// </summary>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="stageType">The stage of User Task.</param>
		/// <param name="result">the result of this user task. If the result is Failure, recommend correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.</param>
		/// <param name="resultSummary">
		/// a summary description for the result.
		/// it provides a little bit more details about the result without digging into it.
		/// when correlated with fault event, use this parameter to summarize the additional information stored in <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.
		/// E.g., "sign in failed because of wrong credential", "user cancelled azure deployment".
		/// Default value is null.
		/// </param>
		internal UserTaskEvent(string eventName, OperationStageType stageType, TelemetryResult result, string resultSummary = null)
			: base(eventName, DataModelEventType.UserTask, stageType, result, resultSummary)
		{
			base.Severity = TelemetrySeverity.High;
		}
	}
}
