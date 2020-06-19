namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Helper class to get information from the Registry
	/// </summary>
	public interface IRegistryTools
	{
		/// <summary>
		/// Get int registry value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		int? GetRegistryIntValueFromLocalMachineRoot(string regKeyPath, string regKeyName, int? defaultOnError = null);

		/// <summary>
		/// Get int registry value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>\
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		int? GetRegistryIntValueFromLocalMachineRoot(string regKeyPath, string regKeyName, bool use64Bit, int? defaultOnError = null);

		/// <summary>
		/// Get registry key value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		object GetRegistryValueFromLocalMachineRoot(string regKeyPath, string regKeyName, object defaultOnError = null);

		/// <summary>
		/// Get registry key value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		object GetRegistryValueFromLocalMachineRoot(string regKeyPath, string regKeyName, bool use64Bit, object defaultOnError = null);

		/// <summary>
		/// Get registry key value from the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		object GetRegistryValueFromCurrentUserRoot(string regKeyPath, string regKeyName, object defaultOnError = null);

		/// <summary>
		/// Sets a value in the registry from the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="value">value to set</param>
		/// <returns>true if set or false if error</returns>
		bool SetRegistryFromCurrentUserRoot(string regKeyPath, string regKeyName, object value);

		/// <summary>
		/// Sets a value in the registry from the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="value">value to set</param>
		/// <param name="use64Bit">optional, if set to true, it uses the 64 bit registry, otherwise defaults to 32 bit</param>
		/// <returns>true if set or false if error</returns>
		bool SetRegistryFromLocalMachineRoot(string regKeyPath, string regKeyName, object value, bool use64Bit = false);
	}
}
