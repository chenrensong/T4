using Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// For FaultEvents, posting to Watson
	/// </summary>
	internal sealed class WatsonSessionChannel : ISessionChannel
	{
		public const string MaxWatsonReportsReached = "VS.Fault.MaximumWatsonReportsReached";

		private TelemetrySession TelemetrySession
		{
			get;
		}

		internal static Random Random
		{
			get;
		} = new Random();


		internal static int NumberOfWatsonReportsThisSession
		{
			get;
			set;
		}

		internal static DateTime DateTimeOfLastWatsonReport
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the percent of expensive operations that will be collected, i.e. 5 means 5% of occurrences will actually create dumps
		/// Can be changed by test code
		/// Reg add HKEY_CURRENT_USER\Software\Coding4Fun\VisualStudio\Telemetry /v FaultEventWatsonSampleRate /t REG_DWORD /d 100 /f
		/// </summary>
		public int FaultEventWatsonSamplePercent
		{
			get;
		}

		public int FaultEventMaximumWatsonReportsPerSession
		{
			get;
		}

		public int FaultEventMinimumSecondsBetweenWatsonReports
		{
			get;
		}

		public string ChannelId => "WatsonChannel";

		public bool IsStarted => true;

		public ChannelProperties Properties
		{
			get;
			set;
		} = ChannelProperties.Default;


		public string TransportUsed => ChannelId;

		public WatsonSessionChannel(TelemetrySession telemetrySession, int faultEventWatsonSamplePercent, int faultEventMaximumWatsonReportsPerSession, int faultEventMinimumSecondsBetweenWatsonReports)
		{
			TelemetrySession = telemetrySession;
			FaultEventWatsonSamplePercent = faultEventWatsonSamplePercent;
			FaultEventMaximumWatsonReportsPerSession = faultEventMaximumWatsonReportsPerSession;
			FaultEventMinimumSecondsBetweenWatsonReports = faultEventMinimumSecondsBetweenWatsonReports;
			DateTimeOfLastWatsonReport = DateTime.MinValue;
		}

		public void PostEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			if (!TelemetrySession.IsOptedIn)
			{
				return;
			}
			FaultEvent faultEvent = telemetryEvent as FaultEvent;
			if (faultEvent == null)
			{
				throw new InvalidOperationException("WatsonSession channel must have FaultEvent posted");
			}
			int num = FaultEventWatsonSamplePercent;
			faultEvent.ReservedProperties["DataModel.Fault.WatsonSamplePercentDefault"] = FaultEventWatsonSamplePercent;
			if (FaultEvent.WatsonSamplePercent.HasValue)
			{
				num = FaultEvent.WatsonSamplePercent.Value;
				faultEvent.ReservedProperties["DataModel.Fault.WatsonSamplePercentOverride"] = FaultEvent.WatsonSamplePercent.Value;
			}
			if (!faultEvent.IsIncludedInWatsonSample.HasValue)
			{
				faultEvent.UserOptInToWatson = FaultEvent.FaultEventWatsonOptIn.Unspecified;
				if (num > 0)
				{
					faultEvent.IsIncludedInWatsonSample = (Random.Next(100) < num);
				}
				else
				{
					faultEvent.IsIncludedInWatsonSample = false;
				}
			}
			else
			{
				if (faultEvent.IsIncludedInWatsonSample == true)
				{
					faultEvent.UserOptInToWatson = FaultEvent.FaultEventWatsonOptIn.PropertyOptIn;
				}
				else
				{
					faultEvent.UserOptInToWatson = FaultEvent.FaultEventWatsonOptIn.PropertyOptOut;
				}
				faultEvent.Properties["VS.Fault.WatsonOptIn"] = faultEvent.UserOptInToWatson.ToString();
			}
			WatsonReport watsonReport = new WatsonReport(faultEvent, TelemetrySession);
			int num2 = FaultEventMaximumWatsonReportsPerSession;
			faultEvent.ReservedProperties["DataModel.Fault.MaxReportsPerSessionDefault"] = FaultEventMaximumWatsonReportsPerSession;
			if (FaultEvent.MaximumWatsonReportsPerSession.HasValue)
			{
				num2 = FaultEvent.MaximumWatsonReportsPerSession.Value;
				faultEvent.ReservedProperties["DataModel.Fault.MaxReportsPerSessionOverride"] = FaultEventMaximumWatsonReportsPerSession;
			}
			if (num == 0 && num2 == 0)
			{
				faultEvent.IsIncludedInWatsonSample = false;
			}
			int minSecondsBetweenReports = FaultEventMinimumSecondsBetweenWatsonReports;
			faultEvent.ReservedProperties["DataModel.Fault.MinSecondsBetweenReportsDefault"] = FaultEventMinimumSecondsBetweenWatsonReports;
			if (FaultEvent.MinimumSecondsBetweenWatsonReports.HasValue)
			{
				minSecondsBetweenReports = FaultEvent.MinimumSecondsBetweenWatsonReports.Value;
				faultEvent.ReservedProperties["DataModel.Fault.MinSecondsBetweenReportsOverride"] = FaultEvent.MinimumSecondsBetweenWatsonReports;
			}
			watsonReport.PostWatsonReport(num2, minSecondsBetweenReports);
		}

		public void PostEvent(TelemetryEvent telemetryEvent, IEnumerable<ITelemetryManifestRouteArgs> args)
		{
			throw new InvalidOperationException("WatsonSession channel doesn't take args on posted");
		}

		public void Start(string sessionId)
		{
		}
	}
}
