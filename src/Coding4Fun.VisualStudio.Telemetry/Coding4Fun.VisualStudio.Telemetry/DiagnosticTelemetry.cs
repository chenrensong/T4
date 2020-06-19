using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Diagnostic telemetry class is intended to gather and post internal telemetry,
	/// i.e. under "VS.TelemetryApi" namespace
	/// </summary>
	internal class DiagnosticTelemetry : IDiagnosticTelemetry
	{
		private readonly ConcurrentDictionary<string, string> registrySettings = new ConcurrentDictionary<string, string>();

		private const string RegistrySettingsPrefix = "VS.TelemetryApi.RegistrySettings.";

		/// <summary>
		/// Log registry settings using key/value pair.
		/// settingsName - suffix for the settings property.
		/// </summary>
		/// <param name="settingsName"></param>
		/// <param name="value"></param>
		public void LogRegistrySettings(string settingsName, string value)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(settingsName, "settingsName");
			CodeContract.RequiresArgumentNotNullAndNotEmpty(value, "value");
			registrySettings["VS.TelemetryApi.RegistrySettings." + settingsName] = value;
		}

		/// <summary>
		/// Post diagnostic telemetry. Generates events with properties and send them.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="propertyBag"></param>
		public void PostDiagnosticTelemetryWhenSessionInitialized(TelemetrySession telemetrySession, IEnumerable<KeyValuePair<string, object>> propertyBag)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			CodeContract.RequiresArgumentNotNull<IEnumerable<KeyValuePair<string, object>>>(propertyBag, "propertyBag");
			if (!telemetrySession.IsSessionCloned)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/Session/Initialized");
				if (registrySettings.Count > 0)
				{
					foreach (KeyValuePair<string, string> registrySetting in registrySettings)
					{
						telemetryEvent.Properties[registrySetting.Key] = registrySetting.Value;
					}
				}
				foreach (KeyValuePair<string, object> item in propertyBag)
				{
					telemetryEvent.Properties[item.Key] = item.Value;
				}
				telemetrySession.PostEvent(telemetryEvent);
			}
		}
	}
}
