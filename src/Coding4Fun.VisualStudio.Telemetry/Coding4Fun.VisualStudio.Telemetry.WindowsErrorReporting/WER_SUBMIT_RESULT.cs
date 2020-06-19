namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	internal enum WER_SUBMIT_RESULT
	{
		WerReportQueued = 1,
		WerReportUploaded,
		WerReportDebug,
		WerReportFailed,
		WerDisabled,
		WerReportCancelled,
		WerDisabledQueue,
		WerReportAsync,
		WerCustomAction,
		WerThrottled
	}
}
