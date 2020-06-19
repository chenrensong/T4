using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// Wrapper class for the Windows Error Reporting functions (not the Mock)
	/// </summary>
	internal sealed class DefaultWerReportImplementation : IWerReport
	{
		public IntPtr WerReportCreateEx(string pwzEventType, WER_REPORT_TYPE repType, IntPtr pReportInformation)
		{
			IntPtr phReportHandle = IntPtr.Zero;
			int num = NativeMethods.WerReportCreate(pwzEventType, repType, pReportInformation, ref phReportHandle);
			if (num != 0)
			{
				throw Marshal.GetExceptionForHR(num);
			}
			return phReportHandle;
		}

		public int WerReportCloseHandle(IntPtr phReportHandle)
		{
			int num = NativeMethods.WerReportCloseHandle(phReportHandle);
			if (num != 0)
			{
				throw Marshal.GetExceptionForHR(num);
			}
			return num;
		}

		public int WerReportSetParameter(IntPtr hReportHandle, int dwparamID, string pwzName, string pwzValue)
		{
			int num = NativeMethods.WerReportSetParameter(hReportHandle, dwparamID, pwzName, pwzValue);
			if (num != 0)
			{
				throw Marshal.GetExceptionForHR(num);
			}
			return num;
		}

		public int WerReportAddDump(IntPtr hReportHandle, IntPtr hProcess, IntPtr hThread, WER_DUMP_TYPE dumpType, IntPtr pExceptionParam, IntPtr pDumpCustomOptions, int dwFlags)
		{
			int num = NativeMethods.WerReportAddDump(hReportHandle, hProcess, hThread, dumpType, pExceptionParam, pDumpCustomOptions, dwFlags);
			if (num != 0)
			{
				throw Marshal.GetExceptionForHR(num);
			}
			return num;
		}

		public int WerReportSubmitEx(IntPtr hReportHandle, WER_CONSENT consent, int dwFlags)
		{
			WER_SUBMIT_RESULT pSubmitResult = (WER_SUBMIT_RESULT)0;
			int num = NativeMethods.WerReportSubmit(hReportHandle, consent, dwFlags, ref pSubmitResult);
			if (num != 0)
			{
				throw Marshal.GetExceptionForHR(num);
			}
			return (int)pSubmitResult;
		}

		public int WerReportAddFile(IntPtr hReportHandle, string pwxPath, WER_FILE_TYPE repFileType, int dwFileFlags)
		{
			int num = NativeMethods.WerReportAddFile(hReportHandle, pwxPath, repFileType, dwFileFlags);
			if (num != 0)
			{
				throw Marshal.GetExceptionForHR(num);
			}
			return num;
		}
	}
}
