using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Diagnostic telemetry class is intended to gather and post internal telemetry,
	/// i.e. under "VS.TelemetryApi" namespace
	/// </summary>
	internal interface IDiagnosticTelemetry
	{
		/// <summary>
		/// Log registry settings using key/value pair
		/// </summary>
		/// <param name="settingsName"></param>
		/// <param name="value"></param>
		void LogRegistrySettings(string settingsName, string value);

		/// <summary>
		/// Post diagnostic telemetry. Generates events with properties and send them.
		/// Adds all properties from the persistent property bag to the event and then
		/// clears the persistent property bag.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="propertyBag"></param>
		void PostDiagnosticTelemetryWhenSessionInitialized(TelemetrySession telemetrySession, IEnumerable<KeyValuePair<string, object>> propertyBag);
	}
}
