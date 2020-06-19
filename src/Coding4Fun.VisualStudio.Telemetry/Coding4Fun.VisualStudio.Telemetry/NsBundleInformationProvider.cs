using Coding4Fun.VisualStudio.Telemetry.Native.Mac;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class NsBundleInformationProvider : INsBundleInformationProvider
	{
		public string GetVersion()
		{
			return MacFoundation.NSBundle.GetVersion();
		}

		public string GetName()
		{
			return MacFoundation.NSBundle.GetBundleName();
		}
	}
}
