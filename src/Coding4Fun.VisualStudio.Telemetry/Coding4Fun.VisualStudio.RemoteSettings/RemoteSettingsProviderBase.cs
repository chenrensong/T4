using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal abstract class RemoteSettingsProviderBase : TelemetryDisposableObject, IRemoteSettingsProvider, ISettingsCollection, IDisposable
	{
		protected IRemoteSettingsStorageHandler currentStorageHandler;

		protected Task<GroupedRemoteSettings> startTask = Task.FromResult<GroupedRemoteSettings>(null);

		protected IRemoteSettingsLogger logger;

		public abstract string Name
		{
			get;
		}

		public RemoteSettingsProviderBase(IRemoteSettingsStorageHandler remoteSettingsStorageHandler, IRemoteSettingsLogger logger = null)
		{
			currentStorageHandler = remoteSettingsStorageHandler;
			this.logger = logger;
		}

		public bool TryGetValueKind(string collectionPath, string key, out ValueKind kind)
		{
			return currentStorageHandler.TryGetValueKind(collectionPath, key, out kind);
		}

		public IEnumerable<string> GetPropertyNames(string collectionPath)
		{
			return currentStorageHandler.GetPropertyNames(collectionPath);
		}

		public IEnumerable<string> GetSubCollectionNames(string collectionPath)
		{
			return currentStorageHandler.GetSubCollectionNames(collectionPath);
		}

		public bool CollectionExists(string collectionPath)
		{
			return currentStorageHandler.CollectionExists(collectionPath);
		}

		public bool PropertyExists(string collectionPath, string propertyName)
		{
			return currentStorageHandler.PropertyExists(collectionPath, propertyName);
		}

		public async Task<RemoteSettingsProviderResult<T>> TryGetValueAsync<T>(string collectionPath, string key)
		{
			RequiresNotDisposed();
			await startTask.ConfigureAwait(false);
			return await currentStorageHandler.TryGetValueAsync<T>(collectionPath, key).ConfigureAwait(false);
		}

		public virtual async Task<IEnumerable<ActionWrapper<T>>> GetActionsAsync<T>(string actionPath)
		{
			RequiresNotDisposed();
			await startTask.ConfigureAwait(false);
			return await Task.FromResult(Enumerable.Empty<ActionWrapper<T>>());
		}

		public virtual async void SubscribeActions<T>(string actionPath, Action<ActionWrapper<T>> callback)
		{
			RequiresNotDisposed();
			await startTask.ConfigureAwait(false);
		}

		public virtual void UnsubscribeActions(string actionPath)
		{
			RequiresNotDisposed();
		}

		public bool TryGetValue<T>(string collectionPath, string key, out T value)
		{
			RequiresNotDisposed();
			return currentStorageHandler.TryGetValue(collectionPath, key, out value);
		}

		public abstract Task<GroupedRemoteSettings> Start();
	}
}
