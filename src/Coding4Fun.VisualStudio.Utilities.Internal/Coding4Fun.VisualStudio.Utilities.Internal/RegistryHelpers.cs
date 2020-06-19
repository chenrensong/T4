using Microsoft.Win32;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	internal class RegistryHelpers
	{
		public static bool TryGetRegistryValueKindForSet(object value, out RegistryValueKind registryValueKind)
		{
			registryValueKind = RegistryValueKind.Unknown;
			if (value.GetType() == typeof(bool))
			{
				registryValueKind = RegistryValueKind.DWord;
				return true;
			}
			return false;
		}
	}
}
