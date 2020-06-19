using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Helper class to get information from the Registry
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class RegistryTools : IRegistryTools3, IRegistryTools2, IRegistryTools
	{
		/// <summary>
		/// Get int registry value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		public int? GetRegistryIntValueFromLocalMachineRoot(string regKeyPath, string regKeyName, int? defaultOnError = null)
		{
			return GetRegistryIntValueFromLocalMachineRoot(regKeyPath, regKeyName, false, defaultOnError);
		}

		/// <summary>
		/// Get int registry value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		public int? GetRegistryIntValueFromLocalMachineRoot(string regKeyPath, string regKeyName, bool use64Bit, int? defaultOnError = null)
		{
			return (int?)GetRegistryValueFromLocalMachineRoot(regKeyPath, regKeyName, use64Bit, defaultOnError);
		}

		/// <summary>
		/// Get registry key value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		public object GetRegistryValueFromLocalMachineRoot(string regKeyPath, string regKeyName, object defaultOnError = null)
		{
			return GetRegistryValueFromLocalMachineRoot(regKeyPath, regKeyName, false, defaultOnError);
		}

		/// <summary>
		/// Get registry key value from the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		public object GetRegistryValueFromLocalMachineRoot(string regKeyPath, string regKeyName, bool use64Bit, object defaultOnError = null)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return GetRegistryValue(rootKey, regKeyPath, regKeyName, defaultOnError);
			}
		}

		/// <summary>
		/// Get registry key value from the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		public object GetRegistryValueFromCurrentUserRoot(string regKeyPath, string regKeyName, object defaultOnError = null)
		{
			return GetRegistryValue(Registry.CurrentUser, regKeyPath, regKeyName, defaultOnError);
		}

		/// <summary>
		/// Determines the kind of a property in the HKCU root registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="kind">current value kind</param>
		/// <returns>True on success, false on failure</returns>
		public bool TryGetRegistryValueKindFromCurrentUserRoot(string regKeyPath, string regKeyName, out RegistryValueKind kind)
		{
			return TryGetRegistryValueKind(Registry.CurrentUser, regKeyPath, regKeyName, out kind);
		}

		/// <summary>
		/// Determines the kind of a property in the HKLM root registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="kind">current value kind</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True on success, false on failure</returns>
		public bool TryGetRegistryValueKindFromLocalMachineRoot(string regKeyPath, string regKeyName, out RegistryValueKind kind, bool use64Bit = false)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return TryGetRegistryValueKind(rootKey, regKeyPath, regKeyName, out kind);
			}
		}

		/// <summary>
		/// Get the names of all values under a key in the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>Array of value names or empty</returns>
		public string[] GetRegistryValueNamesFromCurrentUserRoot(string regKeyPath)
		{
			return GetRegistryValueNames(Registry.CurrentUser, regKeyPath);
		}

		/// <summary>
		/// Get the names of all values under a key in the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>Array of value names or empty</returns>
		public string[] GetRegistryValueNamesFromLocalMachineRoot(string regKeyPath, bool use64Bit = false)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return GetRegistryValueNames(rootKey, regKeyPath);
			}
		}

		/// <summary>
		/// Get the names of all subkeys under a key in the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>Array of value names or empty</returns>
		public string[] GetRegistrySubKeyNamesFromCurrentUserRoot(string regKeyPath)
		{
			return GetRegistrySubKeyNames(Registry.CurrentUser, regKeyPath);
		}

		/// <summary>
		/// Get the names of all subkeys under a key in the HKLM root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>Array of value names or empty</returns>
		public string[] GetRegistrySubKeyNamesFromLocalMachineRoot(string regKeyPath, bool use64Bit = false)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return GetRegistrySubKeyNames(rootKey, regKeyPath);
			}
		}

		/// <summary>
		/// Determines if a key exists in the HKCU root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>True if key exists, false if it does not</returns>
		public bool DoesRegistryKeyExistInCurrentUserRoot(string regKeyPath)
		{
			return DoesRegistryKeyExist(Registry.CurrentUser, regKeyPath);
		}

		/// <summary>
		/// Determines if a key exists in the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True if key exists, false if it does not</returns>
		public bool DoesRegistryKeyExistInLocalMachineRoot(string regKeyPath, bool use64Bit = false)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return DoesRegistryKeyExist(rootKey, regKeyPath);
			}
		}

		/// <summary>
		/// Sets a value in the registry from the HKCU root Registry.
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="value">value to set</param>
		/// <returns>true if set or false if error</returns>
		public bool SetRegistryFromCurrentUserRoot(string regKeyPath, string regKeyName, object value)
		{
			return SetRegistryValue(Registry.CurrentUser, regKeyPath, regKeyName, value);
		}

		/// <summary>
		/// Sets a value in the registry from the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="value">value to set</param>
		/// <param name="use64Bit">optional, if set to true, it uses the 64 bit registry, otherwise defaults to 32 bit</param>
		/// <returns>true if set or false if error</returns>
		public bool SetRegistryFromLocalMachineRoot(string regKeyPath, string regKeyName, object value, bool use64Bit = false)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return SetRegistryValue(rootKey, regKeyPath, regKeyName, value);
			}
		}

		/// <summary>
		/// Deletes the specified registry key and all subkeys in the HKCU root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>True if removed, or false if error</returns>
		public bool DeleteRegistryKeyFromCurrentUserRoot(string regKeyPath)
		{
			return DeleteRegistrySubKey(Registry.CurrentUser, regKeyPath);
		}

		/// <summary>
		/// Deletes the specified registry key and all subkey sin the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True if removed, or false if error</returns>
		public bool DeleteRegistryKeyFromLocalMachineRoot(string regKeyPath, bool use64Bit = false)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return DeleteRegistrySubKey(rootKey, regKeyPath);
			}
		}

		/// <summary>
		/// Deletes the specified registry value from a key in the HKCU root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <returns>True if removed, or false if error</returns>
		public bool DeleteRegistryValueFromCurrentUserRoot(string regKeyPath, string regKeyName)
		{
			return DeleteRegistryValue(Registry.CurrentUser, regKeyPath, regKeyName);
		}

		/// <summary>
		/// Deletes the specified registry value from a key in the HKLM root Registry
		/// </summary>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="use64Bit">if true, it uses the 64 bit registry, otherwise 32 bit is used</param>
		/// <returns>True if removed, or false if error</returns>
		public bool DeleteRegistryValueFromLocalMachineRoot(string regKeyPath, string regKeyName, bool use64Bit = false)
		{
			using (RegistryKey rootKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, use64Bit ? RegistryView.Registry64 : RegistryView.Registry32))
			{
				return DeleteRegistryValue(rootKey, regKeyPath, regKeyName);
			}
		}

		/// <summary>
		/// Get registry key settings int value.
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="defaultOnError">default value on error</param>
		/// <returns>current value or null in case</returns>
		private object GetRegistryValue(RegistryKey rootKey, string regKeyPath, string regKeyName, object defaultOnError = null)
		{
			try
			{
				using (RegistryKey registryKey = rootKey.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.QueryValues))
				{
					if (registryKey == null)
					{
						return defaultOnError;
					}
					object value = registryKey.GetValue(regKeyName);
					if (value != null)
					{
						return value;
					}
					return defaultOnError;
				}
			}
			catch
			{
				return defaultOnError;
			}
		}

		/// <summary>
		/// Get registry value kind.
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="kind">current value kind</param>
		/// <returns>True if no error, otherwise false</returns>
		private bool TryGetRegistryValueKind(RegistryKey rootKey, string regKeyPath, string regKeyName, out RegistryValueKind kind)
		{
			kind = RegistryValueKind.Unknown;
			try
			{
				using (RegistryKey registryKey = rootKey.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.QueryValues))
				{
					if (registryKey != null)
					{
						kind = registryKey.GetValueKind(regKeyName);
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// Get all values under a registry key. If none, an empty array is returned.
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>current value or null in case</returns>
		private string[] GetRegistryValueNames(RegistryKey rootKey, string regKeyPath)
		{
			try
			{
				using (RegistryKey registryKey = rootKey.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.QueryValues))
				{
					if (registryKey != null)
					{
						return registryKey.GetValueNames();
					}
				}
			}
			catch
			{
			}
			return new string[0];
		}

		/// <summary>
		/// Get all subkeys under a registry key. If none, an empty array is returned.
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>current value or null in case</returns>
		private string[] GetRegistrySubKeyNames(RegistryKey rootKey, string regKeyPath)
		{
			try
			{
				using (RegistryKey registryKey = rootKey.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ExecuteKey))
				{
					if (registryKey != null)
					{
						return registryKey.GetSubKeyNames();
					}
				}
			}
			catch
			{
			}
			return new string[0];
		}

		/// <summary>
		/// Checks if registry key exists
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>current value or null in case</returns>
		private bool DoesRegistryKeyExist(RegistryKey rootKey, string regKeyPath)
		{
			try
			{
				using (RegistryKey registryKey = rootKey.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.QueryValues))
				{
					if (registryKey != null)
					{
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// Set registry key settings value.
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <param name="value">value to set</param>
		/// <returns>true if set or false if error</returns>
		private bool SetRegistryValue(RegistryKey rootKey, string regKeyPath, string regKeyName, object value)
		{
			try
			{
				using (RegistryKey registryKey = rootKey.CreateSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
				{
					if (registryKey != null)
					{
						if (RegistryHelpers.TryGetRegistryValueKindForSet(value, out RegistryValueKind registryValueKind))
						{
							registryKey.SetValue(regKeyName, value, registryValueKind);
						}
						else
						{
							registryKey.SetValue(regKeyName, value);
						}
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// Deletes registry subkey and removes all child subkeys.
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <returns>true if deleted or false if error</returns>
		private bool DeleteRegistrySubKey(RegistryKey rootKey, string regKeyPath)
		{
			try
			{
				rootKey.DeleteSubKeyTree(regKeyPath);
				return true;
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// Deletes registry value from the specified subkey.
		/// </summary>
		/// <param name="rootKey">Root key entry</param>
		/// <param name="regKeyPath">Path to the registry key in the format key\subkey\subsubkey</param>
		/// <param name="regKeyName">Variable name under the key</param>
		/// <returns>true if deleted or false if error</returns>
		private bool DeleteRegistryValue(RegistryKey rootKey, string regKeyPath, string regKeyName)
		{
			try
			{
				using (RegistryKey registryKey = rootKey.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.SetValue))
				{
					if (registryKey != null)
					{
						registryKey.DeleteValue(regKeyName);
						return true;
					}
				}
			}
			catch
			{
			}
			return false;
		}
	}
}
