using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	internal static class NativeMethods
	{
		[Flags]
		public enum WERSubmit
		{
			WER_SUBMIT_HONOR_RECOVERY = 0x1,
			WER_SUBMIT_HONOR_RESTART = 0x2,
			WER_SUBMIT_QUEUE = 0x4,
			WER_SUBMIT_SHOW_DEBUG = 0x8,
			WER_SUBMIT_ADD_REGISTERED_DATA = 0x10,
			WER_SUBMIT_OUTOFPROCESS = 0x20,
			WER_SUBMIT_NO_CLOSE_UI = 0x40,
			WER_SUBMIT_NO_QUEUE = 0x80,
			WER_SUBMIT_NO_ARCHIVE = 0x100,
			WER_SUBMIT_START_MINIMIZED = 0x200,
			WER_SUBMIT_OUTOFPROCESS_ASYNC = 0x400,
			WER_SUBMIT_BYPASS_DATA_THROTTLING = 0x800,
			WER_SUBMIT_ARCHIVE_PARAMETERS_ONLY = 0x1000,
			WER_SUBMIT_REPORT_MACHINE_ID = 0x2000
		}

		internal const uint WER_E_LENGTH_EXCEEDED = 2147943683u;

		public const int WER_BUCKETPARAM_MAXLENGTH = 256;

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WerReportCreate(string pwzEventType, WER_REPORT_TYPE repType, IntPtr pReportInformation, ref IntPtr phReportHandle);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WerReportCloseHandle(IntPtr phReportHandle);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WerReportSetParameter(IntPtr hReportHandle, int dwparamID, string pwzName, string pwzValue);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WerReportSetUIOption(IntPtr hReportHandle, WER_REPORT_UI repUITypeID, string pwzValue);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WerReportAddDump(IntPtr hReportHandle, IntPtr hProcess, IntPtr hThread, WER_DUMP_TYPE dumpType, IntPtr pExceptionParam, IntPtr pDumpCustomOptions, int dwFlags);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WerReportSubmit(IntPtr hReportHandle, WER_CONSENT consent, int dwFlags, ref WER_SUBMIT_RESULT pSubmitResult);

		[DllImport("kernel32.dll", ExactSpelling = true)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int WerReportAddFile(IntPtr hReportHandle, string pwxPath, WER_FILE_TYPE repFileType, int dwFileFlags);

		/// <summary>
		/// Really truly non pumping wait.
		/// Raw IntPtrs have to be used, because the marshaller does not support arrays of SafeHandle, only
		/// single SafeHandles.
		/// </summary>
		/// <param name="handleCount">The number of handles in the <paramref name="waitHandles" /> array.</param>
		/// <param name="waitHandles">The handles to wait for.</param>
		/// <param name="waitAll">A flag indicating whether all handles must be signaled before returning.</param>
		/// <param name="millisecondsTimeout">A timeout that will cause this method to return.</param>
		/// <returns></returns>
		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern int WaitForMultipleObjects(uint handleCount, IntPtr[] waitHandles, [MarshalAs(UnmanagedType.Bool)] bool waitAll, uint millisecondsTimeout);
	}
}
