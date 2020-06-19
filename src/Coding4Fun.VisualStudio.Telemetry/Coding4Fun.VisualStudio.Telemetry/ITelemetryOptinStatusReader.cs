namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryOptinStatusReader
	{
		/// <summary>
		/// Read IsOptedIn status for current product.
		/// </summary>
		/// <param name="productVersion">Product version is needed to build a config path</param>
		/// <returns>OptedIn status</returns>
		bool ReadIsOptedInStatus(string productVersion);

		/// <summary>
		/// Calculate IsOptedIn status based on OptedIn status from all installed versions of VS.
		/// </summary>
		/// <param name="session">Host telemetry session</param>
		/// <returns>OptedIn status</returns>
		bool ReadIsOptedInStatus(TelemetrySession session);
	}
}
