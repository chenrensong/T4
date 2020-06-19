using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the successfully loaded manifest if any
		/// </summary>
		public TelemetryManifest TelemetryManifest
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the operation (load/download) succeeded
		/// </summary>
		public bool IsSuccess
		{
			get;
		}

		public TelemetryManifestEventArgs(TelemetryManifest telemetryManifest)
		{
			TelemetryManifest = telemetryManifest;
			IsSuccess = (telemetryManifest != null);
		}
	}
}
