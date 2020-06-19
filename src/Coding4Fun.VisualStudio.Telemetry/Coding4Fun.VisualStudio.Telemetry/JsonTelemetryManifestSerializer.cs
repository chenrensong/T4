using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class JsonTelemetryManifestSerializer : IStreamSerializer
	{
		public async Task SerializeAsync(object objectToSerialize, TextWriter stream)
		{
			_ = (TelemetryManifest)objectToSerialize;
			JsonSerializerSettings val = new JsonSerializerSettings();
			val.NullValueHandling=((NullValueHandling)1);
			JsonSerializerSettings val2 = (JsonSerializerSettings)(object)val;
			await stream.WriteAsync(JsonConvert.SerializeObject(objectToSerialize, (Formatting)1, val2));
		}
	}
}
