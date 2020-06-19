using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public interface ITelemetryService : IDisposable
	{
		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="properties">Optional start event properties</param>
		ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call method <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="properties">Optional start event properties</param>
		ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties);

		/// <summary>
		/// Post a single user task event
		/// </summary>
		void PostUserTask(TelemetryIdentifier telemetryIdentifier, TelemetryResult result);

		/// <summary>
		/// Post a single user task event
		/// </summary>
		/// <param name="properties">Optional event properties</param>
		void PostUserTask(TelemetryIdentifier telemetryIdentifier, TelemetryResult result, IEnumerable<DataPoint> properties);

		/// <summary>
		/// Post a single operation event
		/// </summary>
		void PostOperation(TelemetryIdentifier telemetryIdentifier, TelemetryResult result);

		/// <summary>
		/// Post a single operation event
		/// </summary>
		/// <param name="properties">Optional event properties</param>
		void PostOperation(TelemetryIdentifier telemetryIdentifier, TelemetryResult result, IEnumerable<DataPoint> properties);

		/// <summary>
		/// Reports a single fault event
		/// </summary>
		/// <param name="description">Description of fault</param>
		void PostFault(TelemetryIdentifier telemetryIdentifier, string description);

		/// <summary>
		/// Reports a single fault event
		/// </summary>
		/// <param name="description">Description of fault</param>
		/// <param name="properties">Optional properties of fault</param>0
		void PostFault(TelemetryIdentifier telemetryIdentifier, string description, IEnumerable<DataPoint> properties);

		/// <summary>
		/// Reports a single fault event
		/// </summary>
		/// <param name="description">Description of fault</param>
		/// <param name="exception">Exception object.</param>
		void PostFault(TelemetryIdentifier telemetryIdentifier, string description, Exception exception);

		/// <summary>
		/// Reports a single fault event
		/// </summary>
		/// <param name="description">Description of fault</param>
		/// <param name="exception">Exception object.</param>
		/// <param name="properties">Optional properties of fault</param>0
		void PostFault(TelemetryIdentifier telemetryIdentifier, string description, Exception exception, IEnumerable<DataPoint> properties);
	}
}
