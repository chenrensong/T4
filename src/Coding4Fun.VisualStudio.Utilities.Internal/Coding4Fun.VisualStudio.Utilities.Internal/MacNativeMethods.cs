using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Class containing all PInvoke definitions we use in the Telemetry Library that are used on macOS
	/// </summary>
	public static class MacNativeMethods
	{
		public struct SystemInfo
		{
			public string HardwareMachine;

			public string HardwareModel;

			public long HardwareMemorySize;

			public int HardwarePhysicalCpuSize;

			public int HardwareLogicalCpuSize;

			public string MachineCpuBrandString;
		}

		public struct OSVersionInfo
		{
			public int MajorVersion;

			public int MinorVersion;

			public int BuildNumber;

			public string OSVersion;
		}

		[DllImport("/usr/lib/libSystem.dylib")]
		private static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		private static extern int Gestalt(int selector, out int result);

		[DllImport("libc")]
		private static extern int uname(IntPtr buf);

		public static void GetSystemInfo(ref SystemInfo info)
		{
			info.HardwareLogicalCpuSize = SysctlValueAsInt("hw.logicalcpu_max");
			info.HardwareMachine = SysctlValueAsString("hw.machine");
			info.HardwareMemorySize = SysctlValueAsInt64("hw.memsize");
			info.HardwareModel = SysctlValueAsString("hw.model");
			info.HardwarePhysicalCpuSize = SysctlValueAsInt("hw.physicalcpu_max");
			info.MachineCpuBrandString = SysctlValueAsString("machdep.cpu.brand_string");
		}

		public static void GetOSVersionInfo(ref OSVersionInfo info)
		{
			info.OSVersion = SysctlValueAsString("kern.osversion");
			info.MajorVersion = Gestalt("sys1");
			info.MinorVersion = Gestalt("sys2");
			info.BuildNumber = Gestalt("sys3");
		}

		internal static bool IsRunningOnMac()
		{
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				intPtr = Marshal.AllocHGlobal(8192);
				if (uname(intPtr) == 0 && Marshal.PtrToStringAnsi(intPtr) == "Darwin")
				{
					return true;
				}
			}
			catch
			{
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			return false;
		}

		private static int Gestalt(string selector)
		{
			int result;
			int num = Gestalt((int)(selector[3] | ((uint)selector[2] << 8) | ((uint)selector[1] << 16) | ((uint)selector[0] << 24)), out result);
			if (num != 0)
			{
				throw new Exception($"Error reading gestalt for selector '{selector}': {num}");
			}
			return result;
		}

		private static int SysctlValueAsInt(string name)
		{
			IntPtr intPtr = SysctlGetValue(name);
			try
			{
				return Marshal.ReadInt32(intPtr);
			}
			catch
			{
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			return 0;
		}

		private static long SysctlValueAsInt64(string name)
		{
			IntPtr intPtr = SysctlGetValue(name);
			try
			{
				return Marshal.ReadInt64(intPtr);
			}
			catch
			{
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			return 0L;
		}

		private static string SysctlValueAsString(string name)
		{
			IntPtr intPtr = SysctlGetValue(name);
			try
			{
				return Marshal.PtrToStringAnsi(intPtr);
			}
			catch
			{
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			return string.Empty;
		}

		private static IntPtr SysctlGetValue(string name)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(4);
			Marshal.WriteInt32(intPtr, 0);
			try
			{
				sysctlbyname(name, IntPtr.Zero, intPtr, IntPtr.Zero, 0u);
				int num = Marshal.ReadInt32(intPtr);
				if (num < 1)
				{
					throw new Exception($"sysctl: unknown oid '{name}'");
				}
				IntPtr intPtr2 = Marshal.AllocHGlobal(num);
				if (sysctlbyname(name, intPtr2, intPtr, IntPtr.Zero, 0u) != 0)
				{
					throw new Exception($"sysctlbyname failed for {name}");
				}
				return intPtr2;
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}
}
