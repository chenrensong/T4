using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryJsonLogFile : BaseJsonLogFile<TelemetryEvent>
	{
		/// <summary>
		/// Snapshot object of the telemetry event.
		/// This class is intended to provide plain text event for the
		/// logger purposes.
		/// </summary>
		public sealed class TelemetryLoggerEventSnapshot
		{
			public string Name
			{
				get;
			}

			public IDictionary<string, string> Properties
			{
				get;
			}

			public TelemetryLoggerEventSnapshot(TelemetryEvent telemetryEvent)
			{
				CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
				Name = telemetryEvent.Name;
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				foreach (KeyValuePair<string, object> property in telemetryEvent.Properties)
				{
					dictionary[property.Key] = ((property.Value != null) ? property.Value.ToString() : "null");
				}
				Properties = dictionary;
			}
		}

		public TelemetryJsonLogFile(ITelemetryWriter writer = null)
			: base(writer)
		{
		}

		protected override string ConvertEventToString(TelemetryEvent eventData)
		{
			//IL_000f: Expected O, but got Unknown
			try
			{
				return JsonConvert.SerializeObject((object)new TelemetryLoggerEventSnapshot(eventData));
			}
			catch (JsonSerializationException val)
			{
				JsonSerializationException val2 = (JsonSerializationException)(object)val;
				return string.Format(CultureInfo.InvariantCulture, "{{\"Name\":\"{0}\",\"Error\":\"Cannot serialize event. Error: {1}\"}}", new object[2]
				{
					eventData.Name,
					((Exception)(object)val2).Message
				});
			}
		}
	}
}
