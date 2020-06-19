using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IStableRemoteSettingsProvider : IRemoteSettingsProvider, ISettingsCollection, IDisposable
	{
		bool IsStable(string collectionPath);

		void MakeStable<T>(string collectionPath, string key, T value);
	}
}
