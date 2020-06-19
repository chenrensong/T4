using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class MonoLegacyApi : LegacyApi
	{
		public MonoLegacyApi(IRegistryTools registryTools)
			: base(registryTools)
		{
		}

		protected override string ReadMachineIdFromRegistry(IRegistryTools registry, string regPath, string regKey)
		{
			return (string)registry.GetRegistryValueFromCurrentUserRoot(regPath, regKey, (object)null);
		}

		protected override bool SaveMachineIdToRegistry(IRegistryTools registry, string regPath, string regKey, string machineId)
		{
			return registry.SetRegistryFromCurrentUserRoot(regPath, regKey, (object)machineId);
		}
	}
}
