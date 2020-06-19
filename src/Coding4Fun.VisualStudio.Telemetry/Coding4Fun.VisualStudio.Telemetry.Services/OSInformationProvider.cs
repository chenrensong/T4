using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal class OSInformationProvider : IOSInformationProvider
	{
		private class OSVersionInfo
		{
			public string Version => string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", MajorVersion, MinorVersion, BuildNumber, RevisionNumber);

			public ulong MajorVersion
			{
				get;
				set;
			}

			public ulong MinorVersion
			{
				get;
				set;
			}

			public ulong BuildNumber
			{
				get;
				set;
			}

			public ulong RevisionNumber
			{
				get;
				set;
			}
		}

		private const string OSCurrentVersionRegistryPath = "Software\\Coding4Fun\\Windows NT\\CurrentVersion";

		private const string UBRRegistryKey = "UBR";

		private const string BuildLabRegistryKey = "BuildLabEx";

		private IRegistryTools registryTools;

		public OSInformationProvider(IRegistryTools registryTools)
		{
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(registryTools, "registryTools");
			this.registryTools = registryTools;
		}

		public string GetOSVersion()
		{
			return InitializeOSVersionInfo().Version;
		}

		/// <summary>
		/// Initialize OS Version info structure by system values
		/// We get this information from Win API call to GetVersionEx
		/// </summary>
		/// <returns></returns>
		private OSVersionInfo InitializeOSVersionInfo()
		{
			OSVersionInfo oSVersionInfo = new OSVersionInfo();
			IntPtr intPtr = NativeMethods.LoadLibrary("ntdll.dll");
			if (intPtr != IntPtr.Zero && NativeMethods.GetProcAddress(intPtr, "RtlGetDeviceFamilyInfoEnum") != UIntPtr.Zero)
			{
				ulong pullUAPInfo = 0uL;
				NativeMethods.RtlGetDeviceFamilyInfoEnum(out pullUAPInfo, IntPtr.Zero, IntPtr.Zero);
				oSVersionInfo.MajorVersion = (ulong)((long)pullUAPInfo & -281474976710656L) >> 48;
				oSVersionInfo.MinorVersion = (pullUAPInfo & 0xFFFF00000000) >> 32;
				oSVersionInfo.BuildNumber = (pullUAPInfo & 4294901760u) >> 16;
				oSVersionInfo.RevisionNumber = (pullUAPInfo & 0xFFFF);
				NativeMethods.FreeLibrary(intPtr);
			}
			else
			{
				NativeMethods.OSVersionInfo oSVersionInfo2 = default(NativeMethods.OSVersionInfo);
				oSVersionInfo2.InfoSize = Marshal.SizeOf(typeof(NativeMethods.OSVersionInfo));
				NativeMethods.OSVersionInfo versionInfo = oSVersionInfo2;
				NativeMethods.RtlGetVersion(versionInfo);
				oSVersionInfo.MajorVersion = versionInfo.MajorVersion;
				oSVersionInfo.MinorVersion = versionInfo.MinorVersion;
				oSVersionInfo.BuildNumber = versionInfo.BuildNumber;
				bool flag = false;
				int? registryIntValueFromLocalMachineRoot = registryTools.GetRegistryIntValueFromLocalMachineRoot("Software\\Coding4Fun\\Windows NT\\CurrentVersion", "UBR", (int?)null);
				if (registryIntValueFromLocalMachineRoot.HasValue)
				{
					try
					{
						oSVersionInfo.RevisionNumber = Convert.ToUInt64(registryIntValueFromLocalMachineRoot);
						flag = true;
					}
					catch (FormatException)
					{
					}
				}
				if (!flag)
				{
					object registryValueFromLocalMachineRoot = registryTools.GetRegistryValueFromLocalMachineRoot("Software\\Coding4Fun\\Windows NT\\CurrentVersion", "BuildLabEx", (object)null);
					if (registryValueFromLocalMachineRoot != null && registryValueFromLocalMachineRoot is string)
					{
						string[] array = ((string)registryValueFromLocalMachineRoot).Split('.');
						if (array.Length >= 2)
						{
							try
							{
								oSVersionInfo.RevisionNumber = Convert.ToUInt64(array[1]);
								return oSVersionInfo;
							}
							catch (FormatException)
							{
								return oSVersionInfo;
							}
						}
					}
				}
			}
			return oSVersionInfo;
		}
	}
}
