using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// Interface used to control the CLR
	/// </summary>
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("90F1A06C-7712-4762-86B5-7A5EBA6BDB02")]
	internal interface IClrRuntimeHost
	{
		void Start();

		void Stop();

		void SetHostControl(IntPtr pHostControl);

		IClrControl GetCLRControl();

		void UnloadAppDomain(int dwAppDomainId, bool fWaitUntilDone);

		void ExecuteInAppDomain(int dwAppDomainId, IntPtr pCallback, IntPtr cookie);

		int GetCurrentAppDomainId();

		int ExecuteApplication(string pwzAppFullName, int dwManifestPaths, string[] ppwzManifestPaths, int dwActivationData, string[] ppwzActivationData);

		int ExecuteInDefaultAppDomain(string pwzAssemblyPath, string pwzTypeName, string pwzMethodName, string pwzArgument);
	}
}
