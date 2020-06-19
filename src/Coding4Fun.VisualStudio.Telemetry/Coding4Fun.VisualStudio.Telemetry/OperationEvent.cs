using Coding4Fun.VisualStudio.Utilities.Internal;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class that stores information for operation data model event.
	/// An operation performs some work in application and comes with result (e.g., Success, Failure).
	/// If the operation is invoked by user directly, please use <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or related methods.
	/// A few examples of operations are, license check, package load, windows layout loading.
	///
	/// For long-time running or async operation, in order to understand what else happened during the time or track if it partially completes because of an error,
	/// use method <see cref="M:Coding4Fun.VisualStudio.Telemetry.TelemetrySessionExtensions.StartOperation(Coding4Fun.VisualStudio.Telemetry.TelemetrySession,System.String)" /> which tracks both start and end points.
	/// </summary>
	public class OperationEvent : TelemetryEvent
	{
		private const string OperationPropertyPrefixName = "DataModel.Action.";

		private const string ResultPropertyName = "DataModel.Action.Result";

		private const string ResultSummaryPropertyName = "DataModel.Action.ResultSummary";

		private const string StageTypePropertyName = "DataModel.Action.Type";

		private const string StartTimePropertyName = "DataModel.Action.StartTime";

		private const string EndTimePropertyName = "DataModel.Action.EndTime";

		private const string DurationPropertyName = "DataModel.Action.DurationInMilliseconds";

		private const string PostStartEventPropertyName = "DataModel.Action.PostStartEvent";

		private TelemetryResult result;

		private string resultSummary;

		private OperationStageType stageType;

		private double? duration;

		private long? startTime;

		private long? endTime;

		/// <summary>
		/// Gets result from this operation.
		/// </summary>
		public TelemetryResult Result
		{
			get
			{
				return result;
			}
			private set
			{
				result = value;
				base.ReservedProperties["DataModel.Action.Result"] = TelemetryResultStrings.GetString(value);
			}
		}

		/// <summary>
		/// Gets result summary from this operation.
		/// </summary>
		public string ResultSummary
		{
			get
			{
				return resultSummary;
			}
			private set
			{
				resultSummary = value;
				base.ReservedProperties["DataModel.Action.ResultSummary"] = value;
			}
		}

		/// <summary>
		/// Gets stage type from this operation.
		/// </summary>
		public OperationStageType StageType
		{
			get
			{
				return stageType;
			}
			internal set
			{
				stageType = value;
				base.ReservedProperties["DataModel.Action.Type"] = GetOperationStageTypeName(value);
			}
		}

		/// <summary>
		/// Gets pair id for start-end operation events. It is the same value as CorrelationId.
		/// return null for atomic operation event.
		/// </summary>
		public Guid? StartEndPairId
		{
			get
			{
				Guid? guid = null;
				if (StageType != 0)
				{
					return base.Correlation.Id;
				}
				return guid;
			}
		}

		/// <summary>
		/// Gets duration of the operation if the stage type is End.
		/// Return null for other stage types.
		/// </summary>
		public double? Duration
		{
			get
			{
				return duration;
			}
			private set
			{
				duration = value;
				base.ReservedProperties["DataModel.Action.DurationInMilliseconds"] = value;
			}
		}

		/// <summary>
		/// Gets start time (in ticks) of current operation which stage type is End.
		/// Return null for other stage types.
		/// </summary>
		public long? StartTime
		{
			get
			{
				return startTime;
			}
			private set
			{
				startTime = value;
				if (value.HasValue)
				{
					base.ReservedProperties["DataModel.Action.StartTime"] = new DateTime(value.Value, DateTimeKind.Utc).ToString("O");
				}
				else
				{
					base.ReservedProperties.Remove("DataModel.Action.StartTime");
				}
			}
		}

		/// <summary>
		/// Gets end time (in ticks) of current operation which stage type is End.
		/// Return null for other stage types.
		/// </summary>
		public long? EndTime
		{
			get
			{
				return endTime;
			}
			private set
			{
				endTime = value;
				if (value.HasValue)
				{
					base.ReservedProperties["DataModel.Action.EndTime"] = new DateTime(value.Value, DateTimeKind.Utc).ToString("O");
				}
				else
				{
					base.ReservedProperties.Remove("DataModel.Action.EndTime");
				}
			}
		}

		/// <summary>
		/// Gets product name
		/// </summary>
		public string ProductName => base.ReservedProperties["DataModel.ProductName"].ToString();

		/// <summary>
		/// Gets feature name
		/// </summary>
		public string FeatureName => base.ReservedProperties["DataModel.FeatureName"].ToString();

		/// <summary>
		/// Gets entity name
		/// </summary>
		public string EntityName => base.ReservedProperties["DataModel.EntityName"].ToString();

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> class.
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
		/// This example shows how to create and post <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" />.
		/// <code>
		///     OperationEvent operation = new OperationEvent("vs/debugger/loadingAssembly", Result.Success);
		///     TelemetryService.DefaultSession.PostEvent(operation);
		/// </code>
		/// </example>
		public OperationEvent(string eventName, TelemetryResult result, string resultSummary = null)
			: this(eventName, OperationStageType.Atomic, result, resultSummary)
		{
		}

		/// <summary>
		/// Create an operation event with specified information.
		/// </summary>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="stageType">operation stage type.</param>
		/// <param name="result">the result of this user task. If the result is Failure, recommend correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.</param>
		/// <param name="resultSummary">
		/// a summary description for the result.
		/// it provides a little bit more details about the result without digging into it.
		/// when correlated with fault event, use this parameter to summarize the additional information stored in <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.
		/// E.g., "sign in failed because of wrong credential", "user cancelled azure deployment".
		/// Default value is null.
		/// </param>
		internal OperationEvent(string eventName, OperationStageType stageType, TelemetryResult result, string resultSummary = null)
			: this(eventName, DataModelEventType.Operation, stageType, result, resultSummary)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> class.
		/// </summary>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="eventType">The type of event.</param>
		/// <param name="stageType">The type of operation.</param>
		/// <param name="result">the result of this user task. If the result is Failure, recommend correlate with <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.</param>
		/// <param name="resultSummary">
		/// a summary description for the result.
		/// it provides a little bit more details about the result without digging into it.
		/// when correlated with fault event, use this parameter to summarize the additional information stored in <see cref="T:Coding4Fun.VisualStudio.Telemetry.FaultEvent" />.
		/// E.g., "sign in failed because of wrong credential", "user cancelled azure deployment".
		/// Default value is null.
		/// </param>
		internal OperationEvent(string eventName, DataModelEventType eventType, OperationStageType stageType, TelemetryResult result, string resultSummary = null)
			: base(eventName, TelemetrySeverity.Normal, eventType)
		{
			if (eventType != 0 && eventType != DataModelEventType.Operation)
			{
				throw new ArgumentException("Expect DataModelEventType UserTask or Operation only.", "eventType");
			}
			DataModelEventNameHelper.SetProductFeatureEntityName(this);
			StageType = stageType;
			SetResultProperties(result, resultSummary);
		}

		/// <summary>
		/// Correlate this event with other event via <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation" /> with description information.
		/// </summary>
		/// <param name="correlation">The property <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryEvent.Correlation" /> of correlated event.</param>
		/// <param name="description">
		/// A description string for this correlation information, such as name, hint, tag, category.
		/// Please don't include comma which is a reserved char.
		/// </param>
		/// <remarks>
		/// This method is not thread-safe.
		/// </remarks>
		public void Correlate(TelemetryEventCorrelation correlation, string description)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(description, "description");
			CorrelateWithDescription(correlation, description);
		}

		/// <summary>
		/// Set result related properties
		/// </summary>
		/// <param name="result">the result of this operation</param>
		/// <param name="resultSummary">a summary description for the result. Default value is null.</param>
		internal void SetResultProperties(TelemetryResult result, string resultSummary)
		{
			Result = result;
			ResultSummary = resultSummary;
		}

		/// <summary>
		/// Set time properties for operation event.
		/// </summary>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		/// <param name="durationInMilliseconds"></param>
		internal void SetTimeProperties(DateTime startTime, DateTime endTime, double durationInMilliseconds)
		{
			StartTime = startTime.Ticks;
			EndTime = endTime.Ticks;
			Duration = durationInMilliseconds;
		}

		/// <summary>
		/// Set SendStartEvent property for operation event.
		/// </summary>
		/// <param name="postStartEvent">a boolean indicating whether a start event is posted for this end event.</param>
		internal void SetPostStartEventProperty(bool postStartEvent)
		{
			base.ReservedProperties["DataModel.Action.PostStartEvent"] = postStartEvent;
		}

		private string GetOperationStageTypeName(OperationStageType operationType)
		{
			return operationType.ToString();
		}
	}
}
