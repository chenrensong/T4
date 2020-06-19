namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Interface for the AppInsights Lifecycle Manager.
	/// Manager is necessary for the send signals to the
	/// AI SDK in order to properly handle offline telemetry.
	/// </summary>
	internal interface IAppInsightsLifecycleManager
	{
		/// <summary>
		/// Application starts
		/// </summary>
		void ApplicationStart();

		/// <summary>
		/// Application stops
		/// </summary>
		void ApplicationStop();
	}
}
