namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	internal enum WER_REPORT_TYPE
	{
		/// <summary>
		///  error that is not critical has occurred. This type of report shows no UI; the report is silently queued. It may then be sent silently to the server in the background if adequate user consent is available.
		/// </summary>
		WerReportNonCritical,
		WerReportCritical,
		WerReportApplicationCrash,
		WerReportApplicationHang,
		WerReportKernel,
		WerReportInvalid
	}
}
