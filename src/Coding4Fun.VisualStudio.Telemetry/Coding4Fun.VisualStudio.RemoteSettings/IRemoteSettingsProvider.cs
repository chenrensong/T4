using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IRemoteSettingsProvider : ISettingsCollection, IDisposable
	{
		string Name
		{
			get;
		}

		/// <summary>
		/// Gets current value from storage and indicates if key was found in storage.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns>true if value in storage, default(T) if not in storage</returns>
		bool TryGetValue<T>(string collectionPath, string key, out T value);

		Task<GroupedRemoteSettings> Start();

		Task<IEnumerable<ActionWrapper<T>>> GetActionsAsync<T>(string actionPath);

		void SubscribeActions<T>(string actionPath, Action<ActionWrapper<T>> callback);

		void UnsubscribeActions(string actionPath);

		Task<RemoteSettingsProviderResult<T>> TryGetValueAsync<T>(string collectionPath, string key);
	}
}
