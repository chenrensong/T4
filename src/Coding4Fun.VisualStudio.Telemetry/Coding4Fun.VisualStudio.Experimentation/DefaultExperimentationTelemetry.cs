using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal sealed class DefaultExperimentationTelemetry : IExperimentationTelemetry
	{
		private readonly TelemetrySession telemetrySession;

		public DefaultExperimentationTelemetry(TelemetrySession telemetrySession)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			this.telemetrySession = telemetrySession;
		}

		public void PostEvent(string name, IDictionary<string, string> properties)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(name);
			foreach (KeyValuePair<string, string> property in properties)
			{
				telemetryEvent.Properties[property.Key] = property.Value;
			}
			telemetrySession.PostEvent(telemetryEvent);
		}

		public void SetSharedProperty(string name, string value)
		{
			telemetrySession.SetSharedProperty(name, value);
		}
	}
}
