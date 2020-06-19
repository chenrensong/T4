using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class DefaultTargetedNotificationsTelemetry : ITargetedNotificationsTelemetry
	{
		private TelemetrySession telemetrySession;

		public string SessionId => telemetrySession.SessionId;

		public DefaultTargetedNotificationsTelemetry(TelemetrySession telemetrySession)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			this.telemetrySession = telemetrySession;
		}

		public void PostCriticalFault(string eventName, string description, Exception exception = null, Dictionary<string, object> additionalProperties = null)
		{
			FaultEvent telemetryEvent = new FaultEvent(eventName, description, FaultSeverity.Critical, exception);
			PostEventInternal(telemetryEvent, additionalProperties);
		}

		public void PostDiagnosticFault(string eventName, string description, Exception exception = null, Dictionary<string, object> additionalProperties = null)
		{
			FaultEvent telemetryEvent = new FaultEvent(eventName, description, FaultSeverity.Diagnostic, exception);
			PostEventInternal(telemetryEvent, additionalProperties);
		}

		public void PostGeneralFault(string eventName, string description, Exception exception = null, Dictionary<string, object> additionalProperties = null)
		{
			FaultEvent telemetryEvent = new FaultEvent(eventName, description, FaultSeverity.General, exception);
			PostEventInternal(telemetryEvent, additionalProperties);
		}

		public void PostSuccessfulOperation(string eventName, Dictionary<string, object> additionalProperties = null)
		{
			OperationEvent telemetryEvent = new OperationEvent(eventName, TelemetryResult.Success);
			PostEventInternal(telemetryEvent, additionalProperties);
		}

		private void PostEventInternal(TelemetryEvent telemetryEvent, Dictionary<string, object> additionalProperties = null)
		{
			if (additionalProperties != null)
			{
				foreach (string key in additionalProperties.Keys)
				{
					telemetryEvent.Properties[key] = additionalProperties[key];
				}
			}
			telemetrySession.PostEvent(telemetryEvent);
		}
	}
}
