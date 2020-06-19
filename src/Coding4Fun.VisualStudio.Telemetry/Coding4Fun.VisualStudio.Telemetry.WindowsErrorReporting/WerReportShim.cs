using System;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// Class to allow mocking of pinvoke WER functions
	/// Defaults to DefaultImplementation, which actually calls the WER API
	/// </summary>
	internal class WerReportShim
	{
		private static IWerReport implementation = new DefaultWerReportImplementation();

		/// <summary>
		/// called from tests to override implementation
		/// </summary>
		/// <param name="implementation">if null, restores to default</param>
		internal static void SetImplementation(IWerReport implementation)
		{
			if (implementation == null)
			{
				WerReportShim.implementation = new DefaultWerReportImplementation();
			}
			else
			{
				WerReportShim.implementation = implementation;
			}
		}

		public static IntPtr WerReportCreate(string pwzEventType, WER_REPORT_TYPE repType, IntPtr pReportInformation)
		{
			IntPtr result = IntPtr.Zero;
			try
			{
				result = implementation.WerReportCreateEx(pwzEventType, repType, pReportInformation);
				return result;
			}
			catch (Exception)
			{
				return result;
			}
		}

		public static int WerReportSetParameter(IntPtr hReportHandle, int dwparamID, string pwzName, string pwzValue)
		{
			return implementation.WerReportSetParameter(hReportHandle, dwparamID, pwzName, pwzValue);
		}

		public static int WerReportAddDump(IntPtr hReportHandle, IntPtr hProcess, IntPtr hThread, WER_DUMP_TYPE dumpType, IntPtr pExceptionParam, IntPtr pDumpCustomOptions, int dwFlags)
		{
			return implementation.WerReportAddDump(hReportHandle, hProcess, hThread, dumpType, pExceptionParam, pDumpCustomOptions, dwFlags);
		}

		public static int WerReportAddFile(IntPtr hReportHandle, string pwxPath, WER_FILE_TYPE repFileType, int dwFileFlags)
		{
			return implementation.WerReportAddFile(hReportHandle, pwxPath, repFileType, dwFileFlags);
		}

		public static int WerReportSubmit(IntPtr hReportHandle, WER_CONSENT consent, int dwFlags)
		{
			return implementation.WerReportSubmitEx(hReportHandle, consent, dwFlags);
		}

		public static int WerReportCloseHandle(IntPtr phReportHandle)
		{
			return implementation.WerReportCloseHandle(phReportHandle);
		}
	}
}
