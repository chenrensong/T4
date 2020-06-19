using Newtonsoft.Json.Linq;
using System.IO;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IRemoteSettingsParser
	{
		VersionedDeserializedRemoteSettings TryParseVersionedStream(Stream stream);

		DeserializedRemoteSettings TryParseFromJObject(JObject json, string globalScope = null);
	}
}
