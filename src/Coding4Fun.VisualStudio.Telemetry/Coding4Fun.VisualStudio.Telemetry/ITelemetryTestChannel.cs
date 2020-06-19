namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Interface for the test channels to receive events
	/// </summary>
	public interface ITelemetryTestChannel
	{
		/// <summary>
		/// Process incoming events
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnPostEvent(object sender, TelemetryTestChannelEventArgs e);
	}
}
