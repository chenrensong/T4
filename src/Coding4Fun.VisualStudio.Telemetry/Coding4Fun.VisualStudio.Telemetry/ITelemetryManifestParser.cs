namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Parse manifest stream and create an object
	/// </summary>
	internal interface ITelemetryManifestParser : IStreamParser
	{
		/// <summary>
		/// Parse manifest from the string
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		TelemetryManifest Parse(string jsonString);
	}
}
