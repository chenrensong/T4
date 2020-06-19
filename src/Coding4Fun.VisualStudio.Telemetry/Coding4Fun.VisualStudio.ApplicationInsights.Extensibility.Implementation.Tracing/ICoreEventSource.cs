namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	internal interface ICoreEventSource
	{
		void LogVerbose(string message, string appDomainName = "Incorrect");

		void BuildInfoConfigBrokenXmlError(string msg, string appDomainName = "Incorrect");

		void PopulateRequiredStringWithValue(string parameterName, string telemetryType, string appDomainName = "Incorrect");

		void RequestTelemetryIncorrectDuration(string appDomainName = "Incorrect");

		void LogError(string msg, string appDomainName = "Incorrect");

		void DiagnosticsEventThrottlingHasBeenStartedForTheEvent(int eventId, string appDomainName = "Incorrect");

		void DiagnosticsEventThrottlingHasBeenResetForTheEvent(int eventId, int executionCount, string appDomainName = "Incorrect");

		void DiagnoisticsEventThrottlingSchedulerTimerWasCreated(int intervalInMilliseconds, string appDomainName = "Incorrect");

		void DiagnoisticsEventThrottlingSchedulerTimerWasRemoved(string appDomainName = "Incorrect");

		void DiagnoisticsEventThrottlingSchedulerDisposeTimerFailure(string exception, string appDomainName = "Incorrect");

		void TrackingWasDisabled(string appDomainName = "Incorrect");

		void TrackingWasEnabled(string appDomainName = "Incorrect");

		void TelemetryClientConstructorWithNoTelemetryConfiguration(string appDomainName = "Incorrect");
	}
}
