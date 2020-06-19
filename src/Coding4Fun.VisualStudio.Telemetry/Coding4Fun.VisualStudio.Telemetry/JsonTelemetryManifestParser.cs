using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Parse json manifest stream and create an object
	/// </summary>
	internal class JsonTelemetryManifestParser : ITelemetryManifestParser, IStreamParser
	{
		/// <summary>
		/// Async parse manifest from the text stream
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public async Task<object> ParseAsync(TextReader stream)
		{
			return Parse(await stream.ReadToEndAsync().ConfigureAwait(false));
		}

		/// <summary>
		/// Parse manifest from the string
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		public TelemetryManifest Parse(string jsonString)
		{
			try
			{
				return JsonConvert.DeserializeObject<TelemetryManifest>(jsonString, (JsonConverter[])(object)new JsonConverter[4]
				{
					new JsonTelemetryManifestMatchConverter(),
					new JsonTelemetryManifestMatchValueConverter(),
					new JsonTelemetryManifestActionConverter(),
					new JsonTelemetryManifestRouteArgsConverter()
				});
			}
			catch (Exception innerException)
			{
				throw new TelemetryManifestParserException("there was error in parsing manifest file", innerException);
			}
		}
	}
}
