using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IRemoteSettingsStorageHandler : ISettingsCollection, IScopesStorageHandler
	{
		/// <summary>
		/// Gets current value from storage and indicates if key was found in storage.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath"></param>
		/// <param name="key"></param>
		/// <returns>true if value in storage, default(T) if not in storage</returns>
		Task<RemoteSettingsProviderResult<T>> TryGetValueAsync<T>(string collectionPath, string key);

		bool TryGetValue<T>(string collectionPath, string key, out T value);

		void SaveSettings(GroupedRemoteSettings remoteSettings);

		void SaveNonScopedSetting(RemoteSetting setting);

		void SaveNonScopedSettings(GroupedRemoteSettings groupedSettings);

		void DeleteAllSettings();
	}
}
