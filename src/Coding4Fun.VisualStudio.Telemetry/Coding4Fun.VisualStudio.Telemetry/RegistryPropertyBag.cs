using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Stores a property bag in a registry.
	/// Different exe names do not override each other properties.
	/// </summary>
	internal class RegistryPropertyBag : IPersistentPropertyBag
	{
		private const string KeyPath = "Software\\Microsoft\\VisualStudio\\Telemetry\\PersistentPropertyBag\\";

		private const string StringPrefix = "s:";

		private const string DoublePrefix = "d:";

		private readonly string keyName;

		private readonly string fullKeyName;

		public RegistryPropertyBag(string processName)
		{
			keyName = "Software\\Microsoft\\VisualStudio\\Telemetry\\PersistentPropertyBag\\" + processName;
			fullKeyName = "HKEY_CURRENT_USER\\" + keyName;
		}

		public IEnumerable<KeyValuePair<string, object>> GetAllProperties()
		{
			List<KeyValuePair<string, object>> result = new List<KeyValuePair<string, object>>();
			if (!SafeRegistryCall(delegate
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyName))
				{
					if (registryKey != null)
					{
						string[] valueNames = registryKey.GetValueNames();
						foreach (string text in valueNames)
						{
							object obj = InterpretRegistryValue(registryKey.GetValue(text));
							if (obj != null)
							{
								result.Add(new KeyValuePair<string, object>(text, obj));
							}
						}
					}
				}
			}))
			{
				result.Clear();
			}
			return result;
		}

		public object GetProperty(string propertyName)
		{
			object result = null;
			SafeRegistryCall(delegate
			{
				result = InterpretRegistryValue(Registry.GetValue(fullKeyName, propertyName, null));
			});
			return result;
		}

		public void SetProperty(string propertyName, int value)
		{
			SetProperty(propertyName, (object)value);
		}

		public void SetProperty(string propertyName, string value)
		{
			SetProperty(propertyName, (object)value);
		}

		public void SetProperty(string propertyName, double value)
		{
			SetProperty(propertyName, (object)value);
		}

		public void RemoveProperty(string propertyName)
		{
			SafeRegistryCall(delegate
			{
				using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(keyName, true))
				{
					registryKey?.DeleteValue(propertyName, false);
				}
			});
		}

		public void Clear()
		{
			SafeRegistryCall(delegate
			{
				Registry.CurrentUser.DeleteSubKeyTree(keyName, false);
			});
		}

		protected virtual void SetAccessControl(RegistryKey key)
		{
			RegistryAccessRule rule = new RegistryAccessRule(new SecurityIdentifier("S-1-15-2-1"), RegistryRights.QueryValues | RegistryRights.SetValue | RegistryRights.CreateSubKey | RegistryRights.EnumerateSubKeys | RegistryRights.Notify | RegistryRights.ReadPermissions, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
			RegistrySecurity accessControl = key.GetAccessControl();
			accessControl.AddAccessRule(rule);
			key.SetAccessControl(accessControl);
		}

		private void SetProperty(string propertyName, object value)
		{
			SafeRegistryCall(delegate
			{
				using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(keyName))
				{
					SetAccessControl(registryKey);
					registryKey.SetValue(propertyName, PrepareValueForRegistry(value));
				}
			});
		}

		private static object InterpretRegistryValue(object value)
		{
			string text = value as string;
			if (text != null)
			{
				if (text.StartsWith("d:"))
				{
					if (double.TryParse(text.Substring("d:".Length), out double result))
					{
						return result;
					}
				}
				else if (text.StartsWith("s:"))
				{
					return text.Substring("s:".Length);
				}
			}
			return value;
		}

		private static object PrepareValueForRegistry(object value)
		{
			double? num = value as double?;
			if (num.HasValue)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[2]
				{
					"d:",
					num
				});
			}
			string text = value as string;
			if (text != null)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[2]
				{
					"s:",
					text
				});
			}
			return value;
		}

		/// <summary>
		/// Executes an action that manipulates registry and catches safe exceptions.
		/// A safe exception can be a result of the registry key been deleted by another process
		/// or if a user has manipulated with registry to restrict permissions.
		/// </summary>
		/// <param name="action"></param>
		/// <returns>True is the action was executed and false if the action has thrown an safe registry exception.</returns>
		private bool SafeRegistryCall(Action action)
		{
			try
			{
				action();
				return true;
			}
			catch (Exception ex)
			{
				if (!(ex is IOException) && !(ex is SecurityException) && !(ex is UnauthorizedAccessException) && !(ex is InvalidOperationException))
				{
					throw;
				}
				return false;
			}
		}

		/// <summary>Persists any changes. This is a NoOp for this implementation</summary>
		public void Persist()
		{
		}
	}
}
