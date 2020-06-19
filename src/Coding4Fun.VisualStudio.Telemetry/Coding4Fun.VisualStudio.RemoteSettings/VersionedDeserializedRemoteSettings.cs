using System.Collections.ObjectModel;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class VersionedDeserializedRemoteSettings : DeserializedRemoteSettings
	{
		/// <summary>
		/// Gets file version found in the json file or null of there was an error parsing the josn file.
		/// </summary>
		public string FileVersion
		{
			get;
		}

		/// <summary>
		/// Gets the changeset Id found in json file or null of there was an error parsing the josn file.
		/// </summary>
		public string ChangesetId
		{
			get;
		}

		public VersionedDeserializedRemoteSettings(ReadOnlyCollection<Scope> scopes = null, ReadOnlyCollection<RemoteSetting> settings = null, string fileVersion = null, string changesetId = null, string error = null)
			: base(scopes, settings, error)
		{
			FileVersion = fileVersion;
			ChangesetId = changesetId;
		}

		public VersionedDeserializedRemoteSettings(DeserializedRemoteSettings remoteSettings, string fileVersion = null, string changesetId = null)
			: this(remoteSettings.Scopes, remoteSettings.Settings, fileVersion, changesetId, remoteSettings.Error)
		{
		}
	}
}
