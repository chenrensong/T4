using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("980D2F1A-BF79-4c08-812A-BB9778928F78")]
	internal interface IClrErrorReportingManager
	{
		[PreserveSig]
		int GetBucketParametersForCurrentException(out ClrBucketParameters pParams);

		int BeginCustomDump(int dwFlavoer, int dwNumItems, IntPtr pItems, int dwReserved);

		int EndCustomDump();
	}
}
