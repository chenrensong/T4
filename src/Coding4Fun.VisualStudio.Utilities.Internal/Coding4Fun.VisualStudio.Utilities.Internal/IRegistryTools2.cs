namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	public interface IRegistryTools2 : IRegistryTools
	{
		/// <summary>
		/// Get the names of all values under a key in the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>Array of value names or empty</returns>
		string[] GetRegistryValueNamesFromCurrentUserRoot(string regKeyPath);

		/// <summary>
		/// Get the names of all values under a key in the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>Array of value names or empty</returns>
		string[] GetRegistryValueNamesFromLocalMachineRoot(string regKeyPath, bool use64Bit = false);

		/// <summary>
		/// Get the names of all subkeys under a key in the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>Array of value names or empty</returns>
		string[] GetRegistrySubKeyNamesFromCurrentUserRoot(string regKeyPath);

		/// <summary>
		/// Get the names of all subkeys under a key in the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>Array of value names or empty</returns>
		string[] GetRegistrySubKeyNamesFromLocalMachineRoot(string regKeyPath, bool use64Bit = false);

		/// <summary>
		/// Determines if a key exists in the HKCU root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>True if key exists, false if it does not</returns>
		bool DoesRegistryKeyExistInCurrentUserRoot(string regKeyPath);

		/// <summary>
		/// Determines if a key exists in the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True if key exists, false if it does not</returns>
		bool DoesRegistryKeyExistInLocalMachineRoot(string regKeyPath, bool use64Bit = false);

		/// <summary>
		/// Deletes the specified registry key and all subkeys in the HKCU root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>True if removed, or false if error</returns>
		bool DeleteRegistryKeyFromCurrentUserRoot(string regKeyPath);

		/// <summary>
		/// Deletes the specified registry key and all subkeys in the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True if removed, or false if error</returns>
		bool DeleteRegistryKeyFromLocalMachineRoot(string regKeyPath, bool use64Bit = false);

		/// <summary>
		/// Deletes the specified registry value from a key in the HKCU root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <returns>True if removed, or false if error</returns>
		bool DeleteRegistryValueFromCurrentUserRoot(string regKeyPath, string regKeyName);

		/// <summary>
		/// Deletes the specified registry value from a key in the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True if removed, or false if error</returns>
		bool DeleteRegistryValueFromLocalMachineRoot(string regKeyPath, string regKeyName, bool use64Bit = false);
	}
}
