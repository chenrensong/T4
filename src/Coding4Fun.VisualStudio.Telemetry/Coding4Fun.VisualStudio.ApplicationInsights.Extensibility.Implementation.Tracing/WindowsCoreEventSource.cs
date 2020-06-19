using System.Diagnostics.Tracing;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	[EventSource(Name = "Coding4Fun-ApplicationInsights-Core")]
	internal sealed class WindowsCoreEventSource : EventSource, ICoreEventSource
	{
		/// <summary>
		/// Keywords for the PlatformEventSource.
		/// </summary>
		public sealed class Keywords
		{
			/// <summary>
			/// Key word for user actionable events.
			/// </summary>
			public const EventKeywords UserActionable =(EventKeywords)1L;

			/// <summary>
			/// Keyword for errors that trace at Verbose level.
			/// </summary>
			public const EventKeywords Diagnostics = (EventKeywords)2L;

			/// <summary>
			/// Keyword for errors that trace at Verbose level.
			/// </summary>
			public const EventKeywords VerboseFailure = (EventKeywords)4L;

			/// <summary>
			/// Keyword for errors that trace at Error level.
			/// </summary>
			public const EventKeywords ErrorFailure = (EventKeywords)8L;
		}

		private readonly ApplicationNameProvider nameProvider = new ApplicationNameProvider();

		public static readonly WindowsCoreEventSource Log = new WindowsCoreEventSource();

		[Event(10)]
		public void LogVerbose(string msg, string appDomainName = "Incorrect")
		{
			this.WriteEvent(10, msg ?? string.Empty, nameProvider.Name);
		}

        [Event(20)]
        public void DiagnosticsEventThrottlingHasBeenStartedForTheEvent(int eventId, string appDomainName = "Incorrect")
		{
			this.WriteEvent(20, eventId, nameProvider.Name);
		}

        [Event(30)]
        public void DiagnosticsEventThrottlingHasBeenResetForTheEvent(int eventId, int executionCount, string appDomainName = "Incorrect")
		{
			this.WriteEvent(30, new object[3]
			{
				eventId,
				executionCount,
				nameProvider.Name
			});
		}

        [Event(40)]
        public void DiagnoisticsEventThrottlingSchedulerDisposeTimerFailure(string exception, string appDomainName = "Incorrect")
		{
			this.WriteEvent(40, exception ?? string.Empty, nameProvider.Name);
		}

        [Event(50)]
        public void DiagnoisticsEventThrottlingSchedulerTimerWasCreated(int intervalInMilliseconds, string appDomainName = "Incorrect")
		{
			this.WriteEvent(50, intervalInMilliseconds, nameProvider.Name);
		}

        [Event(60)]
        public void DiagnoisticsEventThrottlingSchedulerTimerWasRemoved(string appDomainName = "Incorrect")
		{
			this.WriteEvent(60, nameProvider.Name);
		}

        [Event(70)]
        public void TelemetryClientConstructorWithNoTelemetryConfiguration(string appDomainName = "Incorrect")
		{
			this.WriteEvent(70, nameProvider.Name);
		}

        [Event(71)]
        public void PopulateRequiredStringWithValue(string parameterName, string telemetryType, string appDomainName = "Incorrect")
		{
			this.WriteEvent(71, parameterName ?? string.Empty, telemetryType ?? string.Empty, nameProvider.Name);
		}

        [Event(72)]
        public void RequestTelemetryIncorrectDuration(string appDomainName = "Incorrect")
		{
			this.WriteEvent(72, nameProvider.Name);
		}

        [Event(80)]
        public void TrackingWasDisabled(string appDomainName = "Incorrect")
		{
			this.WriteEvent(80, nameProvider.Name);
		}

        [Event(81)]
        public void TrackingWasEnabled(string appDomainName = "Incorrect")
		{
			this.WriteEvent(81, nameProvider.Name);
		}

        [Event(90)]
        public void LogError(string msg, string appDomainName = "Incorrect")
		{
			this.WriteEvent(90, msg ?? string.Empty, nameProvider.Name);
		}

        [Event(91)]
        public void BuildInfoConfigBrokenXmlError(string msg, string appDomainName = "Incorrect")
		{
			this.WriteEvent(91, msg ?? string.Empty, nameProvider.Name);
		}

		public WindowsCoreEventSource()
		{
		}
	}
}
