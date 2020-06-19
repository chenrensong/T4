namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// TelemetryNotificationService.Default and TelemetryService.DefaultSession
	/// </summary>
	public interface ISetTelemetrySession
	{
		/// <summary>
		/// set the session to be used
		/// </summary>
		/// <param name="session"></param>
		void SetSession(TelemetrySession session);
	}
}
