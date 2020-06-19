using Coding4Fun.VisualStudio.Telemetry.Native.Mac;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// OS Information Provider supply caller with necessary operating system information, such as
	/// major version, minor version, product type and so on
	/// </summary>
	internal class MacOSPropertyProvider : IPropertyProvider
	{
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
            public string Version => string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", new object[3]
			{
				VersionInfo.MajorVersion,
				VersionInfo.MinorVersion,
				VersionInfo.BuildNumber
			});

			public MacNativeMethods.OSVersionInfo VersionInfo
			{
				get;
				set;
			}

			public string KernelOSVersion => VersionInfo.OSVersion;
		}

		private static readonly long MbInBytes = 1048576L;

		private readonly IEnvironmentTools environmentTools;

		private readonly Lazy<MacFoundation.CoreGraphics.DisplayInformation> displayInfo;

		private readonly Lazy<OSVersionInfo> operatingSystemVersionInfo;

		private readonly Lazy<string> productNameInfo;

		private readonly Lazy<RootDriveInfo> rootDriveInfo;

		private readonly Lazy<long?> totalVolumesSize;

		public MacOSPropertyProvider(IEnvironmentTools envTools)
		{
			CodeContract.RequiresArgumentNotNull<IEnvironmentTools>(envTools, "envTools");
			environmentTools = envTools;
			displayInfo = new Lazy<MacFoundation.CoreGraphics.DisplayInformation>(() => InitializeDisplayInfo(), false);
			operatingSystemVersionInfo = new Lazy<OSVersionInfo>(() => InitializeOSVersionInfo(), false);
			rootDriveInfo = new Lazy<RootDriveInfo>(() => InitializeRootDriveInfo(), false);
			totalVolumesSize = new Lazy<long?>(() => InitializeTotalVolumeSize(), false);
			productNameInfo = new Lazy<string>(() => InitializeProductNameInfo(), false);
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
			if (operatingSystemVersionInfo.Value != null)
			{
				telemetryContext.PostProperty("VS.Core.OS.BuildLab", operatingSystemVersionInfo.Value.KernelOSVersion);
				if (token.IsCancellationRequested)
				{
					return;
				}
			}
			telemetryContext.PostProperty("VS.Core.OS.ClrVersion", environmentTools.Version);
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
			telemetryContext.PostProperty("VS.Core.OS.Display.Count", displayInfo.Value.DisplayCount);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.Resolution", displayInfo.Value.MainDisplayWidth * displayInfo.Value.MainDisplayHeight);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.XY", string.Format(CultureInfo.InvariantCulture, "{0}x{1}", new object[2]
			{
				displayInfo.Value.MainDisplayWidth,
				displayInfo.Value.MainDisplayHeight
			}));
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.OS.Display.VirtualXY", string.Format(CultureInfo.InvariantCulture, "{0}x{1}", new object[2]
			{
				displayInfo.Value.MainDisplayWidth,
				displayInfo.Value.MainDisplayHeight
			}));
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

		private MacFoundation.CoreGraphics.DisplayInformation InitializeDisplayInfo()
		{
			MacFoundation.CoreGraphics.DisplayInformation info = default(MacFoundation.CoreGraphics.DisplayInformation);
			MacFoundation.CoreGraphics.GetDisplayInfo(info);
			return info;
		}

		/// <summary>
		/// Initialize OS Version info structure by system values
		/// We get this information from Win API call to GetVersionEx
		/// </summary>
		/// <returns></returns>
		private OSVersionInfo InitializeOSVersionInfo()
		{
			OSVersionInfo oSVersionInfo = new OSVersionInfo();
			var versionInfo = default(MacNativeMethods.OSVersionInfo);
			MacNativeMethods.GetOSVersionInfo(ref versionInfo);
			oSVersionInfo.VersionInfo = versionInfo;
			return oSVersionInfo;
		}

		private string InitializeProductNameInfo()
		{
			return "macOS";
		}

		private RootDriveInfo InitializeRootDriveInfo()
		{
			try
			{
				DirectoryInfo info = new DirectoryInfo("/");
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
