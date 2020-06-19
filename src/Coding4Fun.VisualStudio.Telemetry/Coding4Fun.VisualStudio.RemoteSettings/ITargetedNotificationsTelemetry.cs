using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Interface that describes the Telemetry operations Remote Settings will use.
	/// Adding a second instance to avoid breaking external consumers.
	/// </summary>
	internal interface ITargetedNotificationsTelemetry
	{
		/// <summary>
		/// Gets the ID of the telemetry session.
		/// </summary>
		string SessionId
		{
			get;
		}

		/// <summary>
		/// Posts a successful operation to telemetry.
		/// </summary>
		/// <param name="eventName">The name of the operation event.</param>
		/// <param name="additionalProperties">Any additional properties to add to the telemetry event.</param>
		void PostSuccessfulOperation(string eventName, Dictionary<string, object> additionalProperties = null);

		/// <summary>
		/// Posts a fault event to telemetry with diagnostic severity.
		/// </summary>
		/// <param name="eventName">The name of the fault event.</param>
		/// <param name="description">The human-readable description of the fault event.</param>
		/// <param name="exception">The exception attributed to the fault, if applicable.</param>
		/// <param name="additionalProperties">Any additional properties to add to the telemetry event.</param>
		void PostDiagnosticFault(string eventName, string description, Exception exception = null, Dictionary<string, object> additionalProperties = null);

		/// <summary>
		/// Posts a fault event to telemetry with general severity.
		/// </summary>
		/// <param name="eventName">The name of the fault event.</param>
		/// <param name="description">The human-readable description of the fault event.</param>
		/// <param name="exception">The exception attributed to the fault, if applicable.</param>
		/// <param name="additionalProperties">Any additional properties to add to the telemetry event.</param>
		void PostGeneralFault(string eventName, string description, Exception exception = null, Dictionary<string, object> additionalProperties = null);

		/// <summary>
		/// Posts a fault event to telemetry with critical severity.
		/// </summary>
		/// <param name="eventName">The name of the fault event.</param>
		/// <param name="description">The human-readable description of the fault event.</param>
		/// <param name="exception">The exception attributed to the fault, if applicable.</param>
		/// <param name="additionalProperties">Any additional properties to add to the telemetry event.</param>
		void PostCriticalFault(string eventName, string description, Exception exception = null, Dictionary<string, object> additionalProperties = null);
	}
}
