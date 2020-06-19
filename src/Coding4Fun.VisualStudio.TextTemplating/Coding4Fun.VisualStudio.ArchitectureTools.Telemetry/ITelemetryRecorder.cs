using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public interface ITelemetryRecorder : IDisposable
	{
		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, ITelemetryScope parentScope);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="properties">Optional start event properties</param>
		ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="properties">Optional start event properties</param>
		ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties, ITelemetryScope parentScope);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, ITelemetryScope parentScope);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="properties">Optional start event properties</param>
		ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties);

		/// <summary>
		/// Start tracking user task by posting a <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> at the beginning of user task work, and then return a <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryScope`1" /> object.
		/// When the user task finishes, call <see cref="M:Coding4Fun.VisualStudio.ArchitectureTools.Telemetry.TelemetryScopeBase.Dispose" /> to post another <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> for end point.
		/// Because the same event name is used by both start and end events, please don't use Start or End in event name.
		/// </summary>
		/// <param name="properties">Optional start event properties</param>
		ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties, ITelemetryScope parentScope);

		void RecordEvent(TelemetryEvent telemetryEvent);
	}
}
