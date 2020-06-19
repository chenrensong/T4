using System.Collections.ObjectModel;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Represents a Json file that has been copied down from Azure.
	/// </summary>
	internal class DeserializedRemoteSettings
	{
		/// <summary>
		/// Gets the collection of scopes that were deserialized from the remote settings json file or null if there
		/// was an error parsing the json file.
		/// </summary>
		public ReadOnlyCollection<Scope> Scopes
		{
			get;
		}

		/// <summary>
		/// Gets the collection of settings that were deserialized from the remote settings json file or null if there
		/// was an error parsing the json file.
		/// </summary>
		public ReadOnlyCollection<RemoteSetting> Settings
		{
			get;
		}

		/// <summary>
		/// Gets the error message that occurred while parsing the json file. If the json file was
		/// parsed successfully this will be null.
		/// </summary>
		public string Error
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the json file was parsed correctly.
		/// </summary>
		public bool Successful => Error == null;

		public DeserializedRemoteSettings(ReadOnlyCollection<Scope> scopes = null, ReadOnlyCollection<RemoteSetting> settings = null, string error = null)
		{
			Scopes = scopes;
			Settings = settings;
			Error = error;
		}
	}
}
