using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class DefaultRemoteSettingsTelemetry : IRemoteSettingsTelemetry
	{
		private class DefaultRemoteSettingsTelemetryActivity : IRemoteSettingsTelemetryActivity
		{
			private readonly TelemetryActivity telemetryActivity;

			private readonly TelemetrySession telemetrySession;

			public DefaultRemoteSettingsTelemetryActivity(TelemetrySession telemetrySession, TelemetryActivity telemetryActivity)
			{
				this.telemetryActivity = telemetryActivity;
				this.telemetrySession = telemetrySession;
			}

			public void End()
			{
				telemetryActivity.End();
			}

			public void Post(IDictionary<string, object> properties)
			{
				foreach (KeyValuePair<string, object> property in properties)
				{
					telemetryActivity.Properties[property.Key] = property.Value;
				}
				telemetrySession.PostEvent(telemetryActivity);
			}

			public void Start()
			{
				telemetryActivity.Start();
			}

			internal TelemetryActivity GetActivity()
			{
				return telemetryActivity;
			}
		}

		private TelemetrySession telemetrySession;

		public DefaultRemoteSettingsTelemetry(TelemetrySession telemetrySession)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			this.telemetrySession = telemetrySession;
		}

		public IRemoteSettingsTelemetryActivity CreateActivity(string name)
		{
			return new DefaultRemoteSettingsTelemetryActivity(telemetrySession, new TelemetryActivity(name));
		}

		public void PostEvent(string name, IDictionary<string, object> properties)
		{
			TelemetryEvent telemetryEvent = new TelemetryEvent(name);
			foreach (KeyValuePair<string, object> property in properties)
			{
				telemetryEvent.Properties[property.Key] = property.Value;
			}
			telemetrySession.PostEvent(telemetryEvent);
		}
	}
}
