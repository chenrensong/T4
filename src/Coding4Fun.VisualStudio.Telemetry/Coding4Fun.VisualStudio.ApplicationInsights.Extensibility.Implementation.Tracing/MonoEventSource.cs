namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	internal sealed class MonoEventSource : ICoreEventSource
	{
		public void BuildInfoConfigBrokenXmlError(string msg, string appDomainName = "Incorrect")
		{
		}

		public void DiagnoisticsEventThrottlingSchedulerDisposeTimerFailure(string exception, string appDomainName = "Incorrect")
		{
		}

		public void DiagnoisticsEventThrottlingSchedulerTimerWasCreated(int intervalInMilliseconds, string appDomainName = "Incorrect")
		{
		}

		public void DiagnoisticsEventThrottlingSchedulerTimerWasRemoved(string appDomainName = "Incorrect")
		{
		}

		public void DiagnosticsEventThrottlingHasBeenResetForTheEvent(int eventId, int executionCount, string appDomainName = "Incorrect")
		{
		}

		public void DiagnosticsEventThrottlingHasBeenStartedForTheEvent(int eventId, string appDomainName = "Incorrect")
		{
		}

		public void LogError(string msg, string appDomainName = "Incorrect")
		{
		}

		public void LogVerbose(string message, string appDomainName = "Incorrect")
		{
		}

		public void PopulateRequiredStringWithValue(string parameterName, string telemetryType, string appDomainName = "Incorrect")
		{
		}

		public void RequestTelemetryIncorrectDuration(string appDomainName = "Incorrect")
		{
		}

		public void TelemetryClientConstructorWithNoTelemetryConfiguration(string appDomainName = "Incorrect")
		{
		}

		public void TrackingWasDisabled(string appDomainName = "Incorrect")
		{
		}

		public void TrackingWasEnabled(string appDomainName = "Incorrect")
		{
		}
	}
}
