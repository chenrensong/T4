using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Interface for telemetry activity.
	/// </summary>
	public interface IRemoteSettingsTelemetryActivity
	{
		/// <summary>
		/// Starts the Telemetry Activity
		/// </summary>
		void Start();

		/// <summary>
		/// Ends the Telemetry Activity
		/// </summary>
		void End();

		/// <summary>
		/// Posts the Telemetry Activity
		/// </summary>
		/// <param name="properties"></param>
		void Post(IDictionary<string, object> properties);
	}
}
