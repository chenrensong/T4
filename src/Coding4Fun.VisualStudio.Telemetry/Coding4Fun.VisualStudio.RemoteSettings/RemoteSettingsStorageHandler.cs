using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class RemoteSettingsStorageHandler : IVersionedRemoteSettingsStorageHandler, IRemoteSettingsStorageHandler, ISettingsCollection, IScopesStorageHandler
	{
		internal class SplitKey
		{
			public string StorageCollectionPath
			{
				get;
			}

			public string StorageKey
			{
				get;
			}

			public string RemoteSettingName
			{
				get;
			}

			public string ScopeString
			{
				get;
			}

			public bool IsScoped => ScopeString != null;

			public SplitKey(string collectionPath, string storageKey)
				: this(collectionPath, storageKey, storageKey, null)
			{
			}

			private SplitKey(string collectionPath, string storageKey, string remoteSettingName, string scopeString)
			{
				StorageCollectionPath = collectionPath;
				StorageKey = storageKey;
				RemoteSettingName = remoteSettingName;
				ScopeString = scopeString;
			}

			/// <summary>
			/// Returns a SplitKey or null if storageName not in correct format.
			/// </summary>
			/// <param name="collectionPath"></param>
			/// <param name="storageName"></param>
			/// <returns></returns>
			public static SplitKey CreateFromStorageName(string collectionPath, string storageName)
			{
				int num = storageName.IndexOf(':');
				if (num == -1)
				{
					return new SplitKey(collectionPath, storageName);
				}
				int num2 = storageName.IndexOf(':', num + 1);
				if (num2 == -1)
				{
					return new SplitKey(collectionPath, storageName, storageName.Substring(num + 1), null);
				}
				return new SplitKey(collectionPath, storageName, storageName.Substring(num + 1, num2 - num - 1), storageName.Substring(num2 + 1));
			}
		}

		private readonly ICollectionKeyValueStorage remoteSettingsStorage;

		private readonly IScopeParserFactory scopeParserFactory;

		private readonly IRemoteSettingsLogger logger;

		private readonly bool isVersioned;

		private const string FileVersionKey = "FileVersion";

		private const string SettingsVersionKey = "SettingsVersion";

		private const string Separator = ":";

		private const string MultipleValueIndicator = "*";

		internal const int SettingsVersion = 2;

		internal readonly string CollectionPathPrefix;

		public string FileVersion
		{
			get
			{
				return remoteSettingsStorage.GetValue(CollectionPathPrefix, "FileVersion", string.Empty);
			}
			set
			{
				remoteSettingsStorage.SetValue(CollectionPathPrefix, "FileVersion", value);
			}
		}

		public int StoredSettingsVersion
		{
			get
			{
				return remoteSettingsStorage.GetValue(CollectionPathPrefix, "SettingsVersion", 0);
			}
			set
			{
				remoteSettingsStorage.SetValue(CollectionPathPrefix, "SettingsVersion", value);
			}
		}

		private string CurrentCollectionPath
		{
			get
			{
				if (isVersioned)
				{
					string fileVersion = FileVersion;
					if (!string.IsNullOrEmpty(fileVersion))
					{
						return Path.Combine(CollectionPathPrefix, fileVersion);
					}
				}
				return CollectionPathPrefix;
			}
		}

		public RemoteSettingsStorageHandler(ICollectionKeyValueStorage storage, string collectionPathPrefix, IScopeParserFactory scopeParserFactory, bool isVersioned, IRemoteSettingsLogger logger)
		{
			CodeContract.RequiresArgumentNotNull<ICollectionKeyValueStorage>(storage, "storage");
			CodeContract.RequiresArgumentNotNull<string>(collectionPathPrefix, "collectionPathPrefix");
			CodeContract.RequiresArgumentNotNull<IScopeParserFactory>(scopeParserFactory, "scopeParserFactory");
			remoteSettingsStorage = storage;
			CollectionPathPrefix = collectionPathPrefix;
			this.isVersioned = isVersioned;
			this.scopeParserFactory = scopeParserFactory;
			this.logger = logger;
		}

		public IEnumerable<string> GetSubCollectionNames(string collectionPath)
		{
			string collectionPath2 = Path.Combine(CurrentCollectionPath, collectionPath);
			return from x in remoteSettingsStorage.GetSubCollectionNames(collectionPath2)
				where !x.EndsWith("*")
				select x;
		}

		public bool CollectionExists(string collectionPath)
		{
			string collectionPath2 = Path.Combine(CurrentCollectionPath, collectionPath);
			return remoteSettingsStorage.CollectionExists(collectionPath2);
		}

		public bool PropertyExists(string collectionPath, string propertyName)
		{
			string text = Path.Combine(CurrentCollectionPath, collectionPath);
			if (!remoteSettingsStorage.PropertyExists(text, propertyName))
			{
				return remoteSettingsStorage.CollectionExists(Path.Combine(text, propertyName + "*"));
			}
			return true;
		}

		public IEnumerable<string> GetPropertyNames(string collectionPath)
		{
			string collectionPath2 = Path.Combine(CurrentCollectionPath, collectionPath);
			IEnumerable<string> propertyNames = remoteSettingsStorage.GetPropertyNames(collectionPath2);
			IEnumerable<string> subCollectionNames = remoteSettingsStorage.GetSubCollectionNames(collectionPath2);
			return propertyNames.Union(from x in subCollectionNames
				where x.EndsWith("*")
				select x.Substring(0, x.Length - 1));
		}

		public async Task<RemoteSettingsProviderResult<T>> TryGetValueAsync<T>(string collectionPath, string key)
		{
			string setting = collectionPath + " $" + key;
			foreach (SplitKey possibleKey in GetPossibleRemoteSettingKeys(collectionPath, key))
			{
				if (possibleKey.RemoteSettingName == key)
				{
					bool flag = possibleKey.IsScoped;
					if (flag)
					{
						flag = !(await EvaluateScopedSettingAsync(new LoggingContext<string>(setting, possibleKey.ScopeString)).ConfigureAwait(false));
					}
					if (!flag)
					{
						T value;
						bool retrievalSuccessful = remoteSettingsStorage.TryGetValue(possibleKey.StorageCollectionPath, possibleKey.StorageKey, out value);
						return new RemoteSettingsProviderResult<T>
						{
							RetrievalSuccessful = retrievalSuccessful,
							Value = value
						};
					}
				}
			}
			return null;
		}

		public bool TryGetValue<T>(string collectionPath, string key, out T value)
		{
			string context = collectionPath + " $" + key;
			foreach (SplitKey possibleRemoteSettingKey in GetPossibleRemoteSettingKeys(collectionPath, key))
			{
				if (possibleRemoteSettingKey.RemoteSettingName == key && (!possibleRemoteSettingKey.IsScoped || EvaluateScopedSetting(new LoggingContext<string>(context, possibleRemoteSettingKey.ScopeString))))
				{
					return remoteSettingsStorage.TryGetValue(possibleRemoteSettingKey.StorageCollectionPath, possibleRemoteSettingKey.StorageKey, out value);
				}
			}
			value = default(T);
			return false;
		}

		public bool TryGetValueKind(string collectionPath, string key, out ValueKind kind)
		{
			string text = Path.Combine(CurrentCollectionPath, collectionPath);
			string fullCollectionPathWithKey = Path.Combine(text, key + "*");
			kind = ValueKind.Unknown;
			if (remoteSettingsStorage.CollectionExists(fullCollectionPathWithKey))
			{
				IEnumerable<string> propertyNames = remoteSettingsStorage.GetPropertyNames(fullCollectionPathWithKey);
				if (propertyNames.Count() != 0)
				{
					if (remoteSettingsStorage.TryGetValueKind(fullCollectionPathWithKey, propertyNames.First(), out ValueKind firstKind))
					{
						if (propertyNames.All((string x) => remoteSettingsStorage.TryGetValueKind(fullCollectionPathWithKey, x, out ValueKind currentKind) && currentKind == firstKind))
						{
							kind = firstKind;
						}
					}
					return true;
				}
			}
			return remoteSettingsStorage.TryGetValueKind(text, key, out kind);
		}

		public bool DoSettingsNeedToBeUpdated(string newFileVersion)
		{
			if (FileVersion.Equals(newFileVersion, StringComparison.InvariantCultureIgnoreCase) && StoredSettingsVersion == 2)
			{
				return !remoteSettingsStorage.CollectionExists(CurrentCollectionPath);
			}
			return true;
		}

		public void DeleteSettingsForFileVersion(string fileVersion)
		{
			string collectionPath = Path.Combine(CollectionPathPrefix, fileVersion);
			if (remoteSettingsStorage.CollectionExists(collectionPath))
			{
				remoteSettingsStorage.DeleteCollection(collectionPath);
			}
		}

		public void SaveNonScopedSetting(RemoteSetting setting)
		{
			if (setting.HasScope)
			{
				throw new InvalidOperationException("Cannot save setting that has scope");
			}
			remoteSettingsStorage.SetValue(setting.Path, setting.Name, setting.Value);
		}

		public void SaveSettings(GroupedRemoteSettings remoteSettings)
		{
			SaveSettingsInternal(CollectionPathPrefix, remoteSettings);
		}

		public void SaveNonScopedSettings(GroupedRemoteSettings groupedSettings)
		{
			foreach (KeyValuePair<string, RemoteSettingPossibilities> groupedSetting in groupedSettings)
			{
				foreach (KeyValuePair<string, List<RemoteSetting>> item in groupedSetting.Value)
				{
					foreach (RemoteSetting item2 in item.Value)
					{
						if (!item2.HasScope)
						{
							remoteSettingsStorage.SetValue(item2.Path, item2.Name, item2.Value);
						}
					}
				}
			}
		}

		public void SaveSettings(VersionedDeserializedRemoteSettings remoteSettings)
		{
			string text = Path.Combine(CollectionPathPrefix, remoteSettings.FileVersion);
			SaveSettingsInternal(text, new GroupedRemoteSettings(remoteSettings, null));
			if (remoteSettings.Scopes != null)
			{
				foreach (Scope scope in remoteSettings.Scopes)
				{
					remoteSettingsStorage.SetValue(Path.Combine(text, "Scopes"), scope.Name, scope.ScopeString);
				}
			}
			if (remoteSettingsStorage.CollectionExists(text))
			{
				FileVersion = remoteSettings.FileVersion;
				StoredSettingsVersion = 2;
			}
		}

		public void CleanUpOldFileVersions(string newFileVersion)
		{
			foreach (string subCollectionName in remoteSettingsStorage.GetSubCollectionNames(CollectionPathPrefix))
			{
				if (subCollectionName != newFileVersion)
				{
					remoteSettingsStorage.DeleteCollection(Path.Combine(CollectionPathPrefix, subCollectionName));
				}
			}
		}

		public void DeleteAllSettings()
		{
			foreach (string subCollectionName in remoteSettingsStorage.GetSubCollectionNames(CollectionPathPrefix))
			{
				remoteSettingsStorage.DeleteCollection(Path.Combine(CollectionPathPrefix, subCollectionName));
			}
			foreach (string propertyName in remoteSettingsStorage.GetPropertyNames(CollectionPathPrefix))
			{
				remoteSettingsStorage.DeleteProperty(CollectionPathPrefix, propertyName);
			}
		}

		public void InvalidateFileVersion()
		{
			remoteSettingsStorage.DeleteProperty(CollectionPathPrefix, "FileVersion");
		}

		public IEnumerable<string> GetAllScopes()
		{
			string collectionPath = Path.Combine(CurrentCollectionPath, "Scopes");
			return remoteSettingsStorage.GetPropertyNames(collectionPath);
		}

		public string GetScope(string scopeName)
		{
			string collectionPath = Path.Combine(CurrentCollectionPath, "Scopes");
			return remoteSettingsStorage.GetValue<string>(collectionPath, scopeName, null);
		}

		private IEnumerable<SplitKey> GetPossibleRemoteSettingKeys(string collectionPath, string key)
		{
			string str = collectionPath + " $" + key;
			string fullCollectionPath = Path.Combine(CurrentCollectionPath, collectionPath);
			string fullCollectionPathWithKey = Path.Combine(fullCollectionPath, key + "*");
			if (remoteSettingsStorage.CollectionExists(fullCollectionPathWithKey))
			{
				logger.LogVerbose(str + " has multiple possible values");
				foreach (string item in from s in remoteSettingsStorage.GetPropertyNames(fullCollectionPathWithKey)
					orderby s
					select s)
				{
					yield return SplitKey.CreateFromStorageName(fullCollectionPathWithKey, item);
				}
			}
			yield return new SplitKey(fullCollectionPath, key);
		}

		private async Task<bool> EvaluateScopedSettingAsync(LoggingContext<string> context)
		{
			bool flag = await scopeParserFactory.EvaluateAsync(context.Value);
			logger.LogVerbose($"Evaluating scope {context.Value} for setting {context.Context}, result: {flag}");
			return flag;
		}

		private bool EvaluateScopedSetting(LoggingContext<string> context)
		{
			bool flag = scopeParserFactory.Evaluate(context.Value);
			logger.LogVerbose($"Evaluating scope {context.Value} for setting {context.Context}, result: {flag}");
			return flag;
		}

		private void SaveSettingsInternal(string newCollectionPath, GroupedRemoteSettings groupedSettings)
		{
			foreach (KeyValuePair<string, RemoteSettingPossibilities> groupedSetting in groupedSettings)
			{
				string text = Path.Combine(newCollectionPath, groupedSetting.Key);
				foreach (KeyValuePair<string, List<RemoteSetting>> item in groupedSetting.Value)
				{
					bool flag = false;
					string text2 = text;
					if (item.Value.Count > 1 || item.Value[0].HasScope)
					{
						flag = true;
						text2 = Path.Combine(text2, item.Key + "*");
					}
					for (int i = 0; i < item.Value.Count; i++)
					{
						RemoteSetting remoteSetting = item.Value[i];
						string text3 = remoteSetting.Name;
						if (flag)
						{
							text3 = i.ToString() + ":" + text3;
						}
						if (remoteSetting.HasScope)
						{
							text3 = text3 + ":" + remoteSetting.ScopeString;
						}
						remoteSettingsStorage.SetValue(text2, text3, remoteSetting.Value);
					}
				}
			}
		}
	}
}
