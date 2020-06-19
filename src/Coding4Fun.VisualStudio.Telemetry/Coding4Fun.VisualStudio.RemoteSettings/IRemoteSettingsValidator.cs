namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IRemoteSettingsValidator
	{
		void ValidateDeserialized(DeserializedRemoteSettings remoteSettings);

		void ValidateStored();
	}
}
