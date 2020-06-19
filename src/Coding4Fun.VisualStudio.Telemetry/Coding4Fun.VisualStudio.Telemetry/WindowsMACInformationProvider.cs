namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class WindowsMACInformationProvider : MACInformationProvider
	{
		private const string Command = "getmac";

		public WindowsMACInformationProvider(IProcessTools processTools, IPersistentPropertyBag persistentStorage)
			: base(processTools, persistentStorage, "getmac", null)
		{
		}
	}
}
