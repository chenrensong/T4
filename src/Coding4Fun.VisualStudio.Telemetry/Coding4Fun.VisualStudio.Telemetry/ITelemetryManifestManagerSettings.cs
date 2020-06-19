namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryManifestManagerSettings
	{
		string BaseUrl
		{
			get;
		}

		string HostId
		{
			get;
		}

		string RelativePath
		{
			get;
		}
	}
}
