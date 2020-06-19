using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryManifestManager : IDisposable
	{
		bool ForcedReadManifest
		{
			get;
		}

		/// <summary>
		/// Event is raised when one of the following is happened:
		/// - new manifest successfully read and parsed
		/// - new manifest successfully downloaded and parsed
		/// - reading manifest failed
		/// - downloading manifest failed
		/// </summary>
		event EventHandler<TelemetryManifestEventArgs> UpdateTelemetryManifestStatusEvent;

		void Start(string hostName, bool isDisposing);

		bool ForceReadManifest();
	}
}
