using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("9065597E-D1A1-4fb2-B6BA-7E1FCE230F61")]
	internal interface IClrControl
	{
		[return: MarshalAs(UnmanagedType.IUnknown)]
		object GetCLRManager([In] ref Guid riid);

		void SetAppDomainManagerType(string pwzAppDomainManagerAssembly, string pwzAppDomainManagerType);
	}
}
