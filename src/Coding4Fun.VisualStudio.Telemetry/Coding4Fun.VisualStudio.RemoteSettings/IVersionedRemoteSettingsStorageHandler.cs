namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IVersionedRemoteSettingsStorageHandler : IRemoteSettingsStorageHandler, ISettingsCollection, IScopesStorageHandler
	{
		string FileVersion
		{
			get;
		}

		void DeleteSettingsForFileVersion(string fileVersion);

		bool DoSettingsNeedToBeUpdated(string newFileVersion);

		void SaveSettings(VersionedDeserializedRemoteSettings remoteSettings);

		void InvalidateFileVersion();

		void CleanUpOldFileVersions(string newFileVersion);
	}
}
