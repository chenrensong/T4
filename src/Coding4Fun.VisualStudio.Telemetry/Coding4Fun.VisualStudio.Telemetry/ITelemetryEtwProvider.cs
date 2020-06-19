namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Implementation for host specific ETW provider for telemetry events.
	/// </summary>
	public interface ITelemetryEtwProvider
	{
		/// <summary>
		/// Writes start event for a TelemetryActivity
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		void WriteActivityStartEvent(TelemetryActivity activity);

		/// <summary>
		/// Writes stop event for a TelemetryActivity
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		void WriteActivityStopEvent(TelemetryActivity activity);

		/// <summary>
		/// Writes event for a TelemetryActivity that was ended with a specified duration
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		void WriteActivityEndWithDurationEvent(TelemetryActivity activity);

		/// <summary>
		/// Writes an event for a TelemetryActivity when it is posted to a session.
		/// </summary>
		/// <param name="activity">Telemetry activity instance</param>
		/// <param name="session"></param>
		void WriteActivityPostEvent(TelemetryActivity activity, TelemetrySession session);

		/// <summary>
		/// Writes an event to indicate a telemetry event being posted to a session
		/// </summary>
		/// <param name="telemetryEvent">Telemetry event instance</param>
		/// <param name="session"></param>
		void WriteTelemetryPostEvent(TelemetryEvent telemetryEvent, TelemetrySession session);
	}
}
