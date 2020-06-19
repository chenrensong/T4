using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Interface that describes the Telemetry operations Remote Settings will use.
	/// </summary>
	public interface IRemoteSettingsTelemetry
	{
		/// <summary>
		/// Posts the named event and properties to Telemetry.
		/// </summary>
		/// <param name="name">name of the event</param>
		/// <param name="properties">dictionary of properties</param>
		void PostEvent(string name, IDictionary<string, object> properties);

		/// <summary>
		/// Creates a Telemetry Activity with the specified name.
		/// </summary>
		/// <param name="name">The name of the activity.</param>
		/// <returns></returns>
		IRemoteSettingsTelemetryActivity CreateActivity(string name);
	}
}
