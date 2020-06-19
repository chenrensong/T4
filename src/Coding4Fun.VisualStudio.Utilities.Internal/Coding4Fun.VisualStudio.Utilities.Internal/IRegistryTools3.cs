using Microsoft.Win32;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	public interface IRegistryTools3 : IRegistryTools2, IRegistryTools
	{
		/// <summary>
		/// Determines the kind of a property in the HKCU root registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="kind">current value kind</param>
		/// <returns>True on success, false on failure</returns>
		bool TryGetRegistryValueKindFromCurrentUserRoot(string regKeyPath, string regKeyName, out RegistryValueKind kind);

		/// <summary>
		/// Determines the kind of a property in the HKLM root registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="kind">current value kind</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True on success, false on failure</returns>
		bool TryGetRegistryValueKindFromLocalMachineRoot(string regKeyPath, string regKeyName, out RegistryValueKind kind, bool use64Bit = false);
	}
}
