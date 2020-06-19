namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class MonoMACInformationProvider : MACInformationProvider
	{
		private const string Command = "ifconfig";

		private const string CommandArgs = "-a";

		public MonoMACInformationProvider(IProcessTools processTools, IPersistentPropertyBag persistentStorage)
			: base(processTools, persistentStorage, "ifconfig", "-a")
		{
		}
	}
}
