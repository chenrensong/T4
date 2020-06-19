using Coding4Fun.VisualStudio.Telemetry.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class StableRemoteSettingsProvider : RemoteSettingsProviderBase, IStableRemoteSettingsProvider, IRemoteSettingsProvider, ISettingsCollection, IDisposable
	{
		private readonly HashSet<string> stableSettingRootSubCollections;

		public override string Name => "StableProvider";

		public StableRemoteSettingsProvider(RemoteSettingsInitializer initializer)
			: base(initializer.LiveRemoteSettingsStorageHandlerFactory())
		{
			stableSettingRootSubCollections = new HashSet<string>(initializer.StableSettingRootSubCollections);
		}

		public bool IsStable(string collectionPath)
		{
			string rootSubCollectionOfPath = collectionPath.GetRootSubCollectionOfPath();
			return stableSettingRootSubCollections.Contains(rootSubCollectionOfPath);
		}

		public void MakeStable<T>(string collectionPath, string key, T value)
		{
			currentStorageHandler.SaveNonScopedSetting(new RemoteSetting(collectionPath, key, value, null));
		}

		public override Task<GroupedRemoteSettings> Start()
		{
			return startTask;
		}
	}
}
