using Coding4Fun.VisualStudio.Experimentation;
using Coding4Fun.VisualStudio.RemoteSettings;
using Coding4Fun.VisualStudio.Utilities.Internal;
using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal sealed class DefaultRegistryKeyValueStorage : ICollectionKeyValueStorage, IKeyValueStorage
	{
		private readonly IRegistryTools3 registryTools;

		public DefaultRegistryKeyValueStorage(IRegistryTools3 registryTools)
		{
			CodeContract.RequiresArgumentNotNull<IRegistryTools3>(registryTools, "registryTools");
			this.registryTools = registryTools;
		}

		public bool CollectionExists(string collectionPath)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			return ((IRegistryTools2)registryTools).DoesRegistryKeyExistInCurrentUserRoot(collectionPath);
		}

		public bool PropertyExists(string collectionPath, string key)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			CodeContract.RequiresArgumentNotNullAndNotEmpty(key, "key");
			return ((IRegistryTools)registryTools).GetRegistryValueFromCurrentUserRoot(collectionPath, key, (object)null) != null;
		}

		public IEnumerable<string> GetPropertyNames(string collectionPath)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			return ((IRegistryTools2)registryTools).GetRegistryValueNamesFromCurrentUserRoot(collectionPath);
		}

		public T GetValue<T>(string key, T defaultValue)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(key, "key");
			Tuple<string, string> pathComponents = GetPathComponents(key);
			GetValueInternal(pathComponents.Item1, pathComponents.Item2, defaultValue, out T value);
			return value;
		}

		public T GetValue<T>(string collectionPath, string key, T defaultValue)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			CodeContract.RequiresArgumentNotNullAndNotEmpty(key, "key");
			GetValueInternal(collectionPath, key, defaultValue, out T value);
			return value;
		}

		public bool TryGetValue<T>(string collectionPath, string key, out T value)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			CodeContract.RequiresArgumentNotNullAndNotEmpty(key, "key");
			return GetValueInternal(collectionPath, key, default(T), out value);
		}

		public bool TryGetValueKind(string collectionPath, string key, out ValueKind kind)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			CodeContract.RequiresArgumentNotNullAndNotEmpty(key, "key");
			kind = ValueKind.Unknown;
			RegistryValueKind registryValueKind = default(RegistryValueKind);
			if (registryTools.TryGetRegistryValueKindFromCurrentUserRoot(collectionPath, key, out registryValueKind))
			{
				switch (registryValueKind)
				{
				case RegistryValueKind.DWord:
					kind = ValueKind.DWord;
					break;
				case RegistryValueKind.QWord:
					kind = ValueKind.QWord;
					break;
				case RegistryValueKind.MultiString:
					kind = ValueKind.MultiString;
					break;
				case RegistryValueKind.String:
					kind = ValueKind.String;
					break;
				default:
					kind = ValueKind.Unknown;
					break;
				}
				return true;
			}
			return false;
		}

		public void SetValue<T>(string key, T value)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(key, "key");
			Tuple<string, string> pathComponents = GetPathComponents(key);
			SetValueInternal(pathComponents.Item1, pathComponents.Item2, value);
		}

		public void SetValue<T>(string collectionPath, string key, T value)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			CodeContract.RequiresArgumentNotNullAndNotEmpty(key, "key");
			SetValueInternal(collectionPath, key, value);
		}

		private Tuple<string, string> GetPathComponents(string key)
		{
			string text = key.NormalizePath();
			int num = text.LastIndexOf('\\');
			if (num == -1)
			{
				throw new ArgumentException("invalid argument 'key'");
			}
			return Tuple.Create(text.Substring(0, num), text.Substring(num + 1));
		}

		private bool GetValueInternal<T>(string collectionPath, string key, T defaultValue, out T value)
		{
			return ((IRegistryTools)registryTools).GetRegistryValueFromCurrentUserRoot(collectionPath, key, (object)null).TryConvertToType(defaultValue, out value);
		}

		private void SetValueInternal<T>(string collectionPath, string key, T value)
		{
			((IRegistryTools)registryTools).SetRegistryFromCurrentUserRoot(collectionPath, key, (object)value);
		}

		public IEnumerable<string> GetSubCollectionNames(string collectionPath)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			return ((IRegistryTools2)registryTools).GetRegistrySubKeyNamesFromCurrentUserRoot(collectionPath);
		}

		public bool DeleteCollection(string collectionPath)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			collectionPath = collectionPath.NormalizePath();
			return ((IRegistryTools2)registryTools).DeleteRegistryKeyFromCurrentUserRoot(collectionPath);
		}

		public bool DeleteProperty(string collectionPath, string propertyName)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(collectionPath, "collectionPath");
			CodeContract.RequiresArgumentNotNullAndNotEmpty(propertyName, "propertyName");
			return ((IRegistryTools2)registryTools).DeleteRegistryValueFromCurrentUserRoot(collectionPath, propertyName);
		}
	}
}
