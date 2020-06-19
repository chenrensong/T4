using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// OS Information Provider supply caller with necessary operating system information, such as
	/// major version, minor version, product type and so on
	/// </summary>
	internal class WindowsOSPropertyProvider : IPropertyProvider
	{
		private struct DisplayInformation
		{
			public int Dpi;

			public float ScalingFactor;
		}

		private class RootDriveInfo
		{
			public long VolumeSize
			{
				get;
				set;
			}

			public long FreeVolumeSpace
			{
				get;
				set;
			}

			public string FileSystem
			{
				get;
				set;
			}
		}

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

		private static readonly long MbInBytes = 1048576L;

		private const string OnValue = "HighContrastModeOn";

		private const string OffValue = "HighContrastModeOff";

		private const string OSCurrentVersionRegistryPath = "Software\\Microsoft\\Windows NT\\CurrentVersion";

		private const string BuildLabRegistryKey = "BuildLabEx";

		private const string CurrentBuildRegistryKey = "CurrentBuildNumber";

		private const string ProductNameRegistryKey = "ProductName";

		private const string UBRRegistryKey = "UBR";

		private const string ClrInstalledVersionRegistryPath = "Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full";

		private const string ReleaseKey = "Release";

		private readonly IEnvironmentTools environmentTools;

		private readonly IRegistryTools registryTools;

		private readonly Lazy<string> buildLabInfo;

		private readonly Lazy<DisplayInformation> displayInfo;

		private readonly Lazy<OSVersionInfo> operatingSystemVersionInfo;

		private readonly Lazy<string> productNameInfo;

		private readonly Lazy<RootDriveInfo> rootDriveInfo;

		private readonly Lazy<long?> totalVolumesSize;

		private readonly Lazy<int> clrInstalledVersion;

		private readonly Lazy<string> clrRunningVersion;

		public WindowsOSPropertyProvider(IEnvironmentTools envTools, IRegistryTools regTools)
		{
			CodeContract.RequiresArgumentNotNull<IEnvironmentTools>(envTools, "envTools");
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(regTools, "regTools");
			environmentTools = envTools;
			registryTools = regTools;
			buildLabInfo = new Lazy<string>(() => InitializeBuildLabInfo(), false);
			displayInfo = new Lazy<DisplayInformation>(() => InitializeDisplayInfo(), false);
			operatingSystemVersionInfo = new Lazy<OSVersionInfo>(() => InitializeOSVersionInfo(), false);
			productNameInfo = new Lazy<string>(() => InitializeProductNameInfo(), false);
			rootDriveInfo = new Lazy<RootDriveInfo>(() => InitializeRootDriveInfo(), false);
			totalVolumesSize = new Lazy<long?>(() => InitializeTotalVolumeSize(), false);
			clrInstalledVersion = new Lazy<int>(() => InitializeClrInstalledVersion(), false);
			clrRunningVersion = new Lazy<string>(() => InitializeClrRunningVersion(), false);
		}

		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.OS.Version", operatingSystemVersionInfo.Value.Version));
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				return;
			}
			if (buildLabInfo.Value != null)
			{
				telemetryContext.PostProperty("VS.Core.OS.BuildLab", buildLabInfo.Value);
				if (token.IsCancellationRequested)
				{
					return;
				}
			}
			telemetryContext.PostProperty("VS.Core.OS.ClrInstalledVersion", clrInstalledVersion.Value);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.ClrRunningVersion", clrRunningVersion.Value);
			if (token.IsCancellationRequested)
			{
				return;
			}
			if (productNameInfo.Value != null)
			{
				telemetryContext.PostProperty("VS.Core.OS.ProductName", productNameInfo.Value);
				if (token.IsCancellationRequested)
				{
					return;
				}
			}
			bool highContrast = SystemInformation.HighContrast;
			telemetryContext.PostProperty("VS.Core.OS.HighContrastId", highContrast);
			if (token.IsCancellationRequested)
			{
				return;
			}
			string propertyValue = highContrast ? "HighContrastModeOn" : "HighContrastModeOff";
			telemetryContext.PostProperty("VS.Core.OS.HighContrastName", propertyValue);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.Dpi", displayInfo.Value.Dpi);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.ScalingFactor", displayInfo.Value.ScalingFactor);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.Count", SystemInformation.MonitorCount);
			if (token.IsCancellationRequested)
			{
				return;
			}
			Size primaryMonitorSize = SystemInformation.PrimaryMonitorSize;
			telemetryContext.PostProperty("VS.Core.OS.Display.Resolution", primaryMonitorSize.Width * primaryMonitorSize.Height);
			if (token.IsCancellationRequested)
			{
				return;
			}
			Rectangle virtualScreen = SystemInformation.VirtualScreen;
			telemetryContext.PostProperty("VS.Core.OS.Display.XY", string.Format(CultureInfo.InvariantCulture, "{0}x{1}", new object[2]
			{
				primaryMonitorSize.Width,
				primaryMonitorSize.Height
			}));
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.VirtualXY", string.Format(CultureInfo.InvariantCulture, "{0}x{1}", new object[2]
			{
				virtualScreen.Width,
				virtualScreen.Height
			}));
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.ColorDepth", Screen.PrimaryScreen.BitsPerPixel);
			if (token.IsCancellationRequested)
			{
				return;
			}
			if (totalVolumesSize.Value.HasValue)
			{
				telemetryContext.PostProperty("VS.Core.OS.Drive.AllVolumesSize", totalVolumesSize.Value);
				if (token.IsCancellationRequested)
				{
					return;
				}
			}
			if (rootDriveInfo.Value == null)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Drive.VolumeSize", rootDriveInfo.Value.VolumeSize);
			if (!token.IsCancellationRequested)
			{
				telemetryContext.PostProperty("VS.Core.OS.Drive.FreeVolumeSpace", rootDriveInfo.Value.FreeVolumeSpace);
				if (!token.IsCancellationRequested)
				{
					telemetryContext.PostProperty("VS.Core.OS.Drive.FileSystem", rootDriveInfo.Value.FileSystem);
				}
			}
		}

		private string InitializeBuildLabInfo()
		{
			object registryValueFromLocalMachineRoot = registryTools.GetRegistryValueFromLocalMachineRoot("Software\\Microsoft\\Windows NT\\CurrentVersion", "BuildLabEx", (object)null);
			if (registryValueFromLocalMachineRoot != null && registryValueFromLocalMachineRoot is string)
			{
				return (string)registryValueFromLocalMachineRoot;
			}
			return null;
		}

		private int InitializeClrInstalledVersion()
		{
			return registryTools.GetRegistryIntValueFromLocalMachineRoot("Software\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full", "Release", (int?)null) ?? 0;
		}

		private string InitializeClrRunningVersion()
		{
			return (from m in Process.GetCurrentProcess().Modules.OfType<ProcessModule>()
				where string.Equals(m.ModuleName, "clr.dll", StringComparison.OrdinalIgnoreCase)
				select m).FirstOrDefault()?.FileVersionInfo.FileVersion ?? "Unknown";
		}

		private DisplayInformation InitializeDisplayInfo()
		{
			DisplayInformation result = default(DisplayInformation);
			using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
			{
				IntPtr hdc = graphics.GetHdc();
				int deviceCaps = NativeMethods.GetDeviceCaps(hdc, 10);
				int deviceCaps2 = NativeMethods.GetDeviceCaps(hdc, 117);
				result.ScalingFactor = (float)deviceCaps2 / (float)deviceCaps;
				result.Dpi = NativeMethods.GetDeviceCaps(hdc, 90);
				graphics.ReleaseHdc(hdc);
				return result;
			}
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
				int? registryIntValueFromLocalMachineRoot = registryTools.GetRegistryIntValueFromLocalMachineRoot("Software\\Microsoft\\Windows NT\\CurrentVersion", "UBR", (int?)null);
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
					object registryValueFromLocalMachineRoot = registryTools.GetRegistryValueFromLocalMachineRoot("Software\\Microsoft\\Windows NT\\CurrentVersion", "BuildLabEx", (object)null);
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

		private string InitializeProductNameInfo()
		{
			object registryValueFromLocalMachineRoot = registryTools.GetRegistryValueFromLocalMachineRoot("Software\\Microsoft\\Windows NT\\CurrentVersion", "ProductName", (object)null);
			if (registryValueFromLocalMachineRoot != null && registryValueFromLocalMachineRoot is string)
			{
				return (string)registryValueFromLocalMachineRoot;
			}
			return null;
		}

		private RootDriveInfo InitializeRootDriveInfo()
		{
			try
			{
				DirectoryInfo info = new DirectoryInfo(".");
				DriveInfo driveInfo = DriveInfo.GetDrives().FirstOrDefault((DriveInfo x) => x.IsReady && x.DriveType == DriveType.Fixed && x.Name.Equals(info.Root.Name, StringComparison.InvariantCultureIgnoreCase));
				if (driveInfo != null)
				{
					return new RootDriveInfo
					{
						VolumeSize = driveInfo.TotalSize / MbInBytes,
						FreeVolumeSpace = driveInfo.AvailableFreeSpace / MbInBytes,
						FileSystem = driveInfo.DriveFormat
					};
				}
			}
			catch (IOException)
			{
			}
			catch (SecurityException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			return null;
		}

		private long? InitializeTotalVolumeSize()
		{
			try
			{
				return (from x in DriveInfo.GetDrives()
					where x.IsReady && x.DriveType == DriveType.Fixed
					select x).Sum((DriveInfo y) => y.TotalSize) / MbInBytes;
			}
			catch (IOException)
			{
			}
			catch (SecurityException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			return null;
		}
	}
}
