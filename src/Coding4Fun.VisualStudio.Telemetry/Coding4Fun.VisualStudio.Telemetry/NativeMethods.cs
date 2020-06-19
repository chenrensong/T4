using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Class containing all PInvoke definitions we use in the Telemetry Library (other than SQM API) and some helpers related to native method calls.
	/// </summary>
	internal static class NativeMethods
	{
		internal struct OSVersionInfo
		{
			public int InfoSize;

			public uint MajorVersion;

			public uint MinorVersion;

			public uint BuildNumber;

			public uint PlatformId;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
			public string StringVersion;

			public short ServicePackMajor;

			public short ServicePackMinor;

			public short SuiteMask;

			public byte ProductType;

			public byte Reserved;
		}

		internal struct MemoryStatus
		{
			public uint Length;

			public uint MemoryLoad;

			public ulong TotalPhys;

			public ulong AvailPhys;

			public ulong TotalPageFile;

			public ulong AvailPageFile;

			public ulong TotalVirtual;

			public ulong AvailVirtual;

			public ulong AvailExtendedVirtual;
		}

		internal struct SystemInfo
		{
			public ushort ProcessorArchitecture;

			public ushort Reserved;

			public uint PageSize;

			public UIntPtr MinimumApplicationAddress;

			public UIntPtr MaximumApplicationAddress;

			public UIntPtr ActiveProcessorMask;

			public uint NumberOfProcessors;

			public uint ProcessorType;

			public uint AllocationGranularity;

			public ushort ProcessorLevel;

			public ushort ProcessorRevision;
		}

		/// <summary>
		/// OS Feature Flags list copied from shlwapi.h
		/// </summary>
		internal enum OSFeatureFlag
		{
			OSDomainMember = 28
		}

		internal struct DisplayInformation
		{
			public int Dpi;

			public float ScalingFactor;
		}

		internal enum FirmwareTableProviderSignature
		{
			ACPI = 1094930505,
			FIRM = 1179210317,
			RSMB = 1381190978
		}

		/// <summary>
		/// Returns the EXE file name without extension of the current process. In case of error, a null is returned.
		/// </summary>
		/// <returns></returns>
		internal static string GetFullProcessExeName()
		{
			StringBuilder stringBuilder = new StringBuilder(1000);
			GetModuleFileName(IntPtr.Zero, stringBuilder, stringBuilder.Capacity);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Retrieves the process identifier of the calling process.
		/// </summary>
		/// <returns>The process id</returns>
		[DllImport("kernel32.dll")]
		internal static extern uint GetCurrentProcessId();

		/// <summary>
		/// Retrieves the current thread id.
		/// </summary>
		/// <returns>The thread id</returns>
		[DllImport("kernel32.dll")]
		internal static extern uint GetCurrentThreadId();

		[DllImport("kernel32.dll")]
		internal static extern bool IsDebuggerPresent();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern uint GetModuleFileName([In] IntPtr handleModule, [Out] StringBuilder filename, [In] [MarshalAs(UnmanagedType.U4)] int size);

		[DllImport("kernel32.dll")]
		internal static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		internal static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		internal static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("ntdll.dll")]
		internal static extern bool RtlGetVersion(OSVersionInfo versionInfo);

		[DllImport("ntdll.dll")]
		internal static extern bool RtlGetDeviceFamilyInfoEnum(out ulong pullUAPInfo, IntPtr pullDeviceFamily, IntPtr pullDeviceForm);

		[DllImport("kernel32.dll")]
		internal static extern bool GlobalMemoryStatusEx(MemoryStatus bufferPointer);

		[DllImport("kernel32.dll")]
		internal static extern bool GetNativeSystemInfo(SystemInfo systemInfo);

		[DllImport("shlwapi.dll")]
		internal static extern bool IsOS(OSFeatureFlag featureFlag);

		[DllImport("gdi32.dll")]
		internal static extern int GetDeviceCaps(IntPtr hdc, int index);

		[DllImport("advapi32.dll")]
		internal static extern bool GetTokenInformation(IntPtr tokenHandle, int tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength);

		/// <summary>
		/// Returns the UTC start time in Ticks of the current process or null if the start time cannot be determined.
		/// </summary>
		/// <returns></returns>
		internal static long? GetProcessCreationTime()
		{
			if (GetProcessTimes(GetCurrentProcess(), out System.Runtime.InteropServices.ComTypes.FILETIME creationTime, out System.Runtime.InteropServices.ComTypes.FILETIME _, out System.Runtime.InteropServices.ComTypes.FILETIME _, out System.Runtime.InteropServices.ComTypes.FILETIME _))
			{
				return FiletimeToDateTime(creationTime).Ticks;
			}
			return null;
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetProcessTimes(IntPtr handleProcess, out System.Runtime.InteropServices.ComTypes.FILETIME creationTime, out System.Runtime.InteropServices.ComTypes.FILETIME exitTime, out System.Runtime.InteropServices.ComTypes.FILETIME kernelTime, out System.Runtime.InteropServices.ComTypes.FILETIME userTime);

		private static DateTime FiletimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
		{
			return DateTime.FromFileTimeUtc((long)(((ulong)(uint)fileTime.dwHighDateTime << 32) | (uint)fileTime.dwLowDateTime));
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int EnumSystemFirmwareTables(FirmwareTableProviderSignature firmwareTableProviderSignature, IntPtr firmwareTableBuffer, int bufferSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern int GetSystemFirmwareTable(FirmwareTableProviderSignature firmwareTableProviderSignature, int firmwareTableID, IntPtr firmwareTableBuffer, int bufferSize);
	}
}
