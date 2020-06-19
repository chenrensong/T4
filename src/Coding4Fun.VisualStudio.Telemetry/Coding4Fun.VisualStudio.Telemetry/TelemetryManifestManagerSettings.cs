using System.Globalization;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestManagerSettings : ITelemetryManifestManagerSettings
	{
		private readonly string urlFilePath;

		private readonly string urlFilePattern = "v{0}/dyntelconfig.json";

		public string BaseUrl => "https://az667904.vo.msecnd.net/pub";

		public string HostId
		{
			get;
		}

		public string RelativePath => urlFilePath;

		public TelemetryManifestManagerSettings(string hostName, string theUrlFilePattern = null)
		{
			if (theUrlFilePattern != null)
			{
				urlFilePattern = theUrlFilePattern;
			}
			HostId = hostName;
			urlFilePath = string.Format(CultureInfo.InvariantCulture, urlFilePattern, new object[1]
			{
				2u
			});
		}
	}
}
