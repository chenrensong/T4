namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Dump type https://msdn.microsoft.com/en-us/library/windows/desktop/bb513622(v=vs.85).aspx
	/// </summary>
	public enum WER_DUMP_TYPE
	{
		/// <summary>
		/// A limited minidump that contains only a stack trace.
		/// This type is equivalent to creating a minidump with the following options:
		/// •MiniDumpWithDataSegs
		/// •MiniDumpWithUnloadedModules
		/// •MiniDumpWithProcessThreadData
		/// •MiniDumpWithoutOptionalData
		/// </summary>
		WerDumpTypeMicroDump = 1,
		/// <summary>
		/// A minidump.
		///
		/// This type is equivalent to creating a minidump with the following options:
		/// •MiniDumpWithDataSegs
		/// •MiniDumpWithUnloadedModules
		/// •MiniDumpWithProcessThreadData
		/// •MiniDumpWithTokenInformation (Windows 7 and later)
		/// </summary>
		WerDumpTypeMiniDump,
		/// <summary>
		/// An extended minidump that contains additional data such as the process memory.
		///
		/// This type is equivalent to creating a minidump with the following options:
		/// •MiniDumpWithDataSegs
		/// •MiniDumpWithProcessThreadData
		/// •MiniDumpWithHandleData
		/// •MiniDumpWithPrivateReadWriteMemory
		/// •MiniDumpWithUnloadedModules
		/// •MiniDumpWithFullMemoryInfo
		/// •MiniDumpWithThreadInfo (Windows 7 and later)
		/// •MiniDumpWithTokenInformation (Windows 7 and later)
		/// •MiniDumpWithPrivateWriteCopyMemory (Windows 7 and later)
		/// </summary>
		WerDumpTypeHeapDump,
		/// <summary>
		/// not in MSDN yet
		/// </summary>
		WerDumpTypeTriageDump,
		/// <summary>
		/// Max
		/// </summary>
		WerDumpTypeMax
	}
}
