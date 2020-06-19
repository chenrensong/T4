namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IRemoteSettingsStorageHandlerFactory
	{
		IVersionedRemoteSettingsStorageHandler BuildVersioned(ICollectionKeyValueStorage storage, bool usePrefix, string fileName, IScopeParserFactory scopeParserFactory);

		IRemoteSettingsStorageHandler Build(ICollectionKeyValueStorage storage, bool usePrefix, RemoteSettingsFilterProvider filterProvider, IScopeParserFactory scopeParserFactory);

		IRemoteSettingsStorageHandler Build(ICollectionKeyValueStorage storage, bool usePrefix, string collectionPath, IScopeParserFactory scopeParserFactory);
	}
}
