using Coding4Fun.VisualStudio.Telemetry;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class RemoteSettingsJsonLogFile : BaseJsonLogFile<RemoteSettingsLogger.RemoteSettingsLogMessage>
	{
		public RemoteSettingsJsonLogFile(ITelemetryWriter writer = null)
			: base(writer)
		{
		}

		protected override string ConvertEventToString(RemoteSettingsLogger.RemoteSettingsLogMessage eventData)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			//IL_0016: Expected O, but got Unknown
			try
			{
				JsonSerializerSettings val = new JsonSerializerSettings();
				val.NullValueHandling=((NullValueHandling)1);
				return JsonConvert.SerializeObject((object)eventData, (JsonSerializerSettings)(object)val);
			}
			catch (JsonSerializationException val2)
			{
				JsonSerializationException val3 = (JsonSerializationException)(object)val2;
				return string.Format(CultureInfo.InvariantCulture, "{{\"Name\":\"{0}\",\"Error\":\"Cannot serialize log message. Error: {1}\"}}", new object[2]
				{
					eventData.Message,
					((Exception)(object)val3).Message
				});
			}
		}
	}
}
