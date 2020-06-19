namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A null etw provider that does nothing with event calls.
	/// </summary>
	internal sealed class TelemetryNullEtwProvider : ITelemetryEtwProvider
	{
		public void WriteActivityEndWithDurationEvent(TelemetryActivity activity)
		{
		}

		public void WriteActivityPostEvent(TelemetryActivity activity, TelemetrySession session)
		{
		}

		public void WriteActivityStartEvent(TelemetryActivity activity)
		{
		}

		public void WriteActivityStopEvent(TelemetryActivity activity)
		{
		}

		public void WriteTelemetryPostEvent(TelemetryEvent telemetryEvent, TelemetrySession session)
		{
		}
	}
}
