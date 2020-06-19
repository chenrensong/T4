namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryLogFile<T>
	{
		/// <summary>
		/// Ensure to create a new writer for each session
		/// </summary>
		/// <param name="settingsProvider"></param>
		void Initialize(ITelemetryLogSettingsProvider settingsProvider);

		/// <summary>
		/// Write event to file
		/// </summary>
		/// <param name="telemetryEvent"></param>
		void WriteAsync(T telemetryEvent);
	}
}
