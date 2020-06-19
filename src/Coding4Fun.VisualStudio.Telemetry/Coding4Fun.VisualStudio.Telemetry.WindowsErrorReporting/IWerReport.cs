using System;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	internal interface IWerReport
	{
		IntPtr WerReportCreateEx(string pwzEventType, WER_REPORT_TYPE repType, IntPtr pReportInformation);

		int WerReportSetParameter(IntPtr hReportHandle, int dwparamID, string pwzName, string pwzValue);

		int WerReportAddDump(IntPtr hReportHandle, IntPtr hProcess, IntPtr hThread, WER_DUMP_TYPE dumpType, IntPtr pExceptionParam, IntPtr pDumpCustomOptions, int dwFlags);

		int WerReportAddFile(IntPtr hReportHandle, string pwxPath, WER_FILE_TYPE repFileType, int dwFileFlags);

		int WerReportSubmitEx(IntPtr hReportHandle, WER_CONSENT consent, int dwFlags);

		int WerReportCloseHandle(IntPtr phReportHandle);
	}
}
