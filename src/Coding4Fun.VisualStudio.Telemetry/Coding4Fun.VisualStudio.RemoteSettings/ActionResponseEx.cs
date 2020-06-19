using Coding4Fun.VisualStudio.Telemetry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal static class ActionResponseEx
	{
		private static readonly Lazy<JsonConverter[]> matchConvertersLazy = new Lazy<JsonConverter[]>(() => (JsonConverter[])(object)new JsonConverter[2]
		{
			new JsonTelemetryEventMatchConverter(),
			new JsonTelemetryManifestMatchValueConverter()
		});

		public static ActionWrapper<T> AsTypedAction<T>(this ActionResponse actionResponse)
		{
			try
			{
				T action = JsonConvert.DeserializeObject<T>(actionResponse.ActionJson, matchConvertersLazy.Value);
				return new ActionWrapper<T>
				{
					RuleId = actionResponse.RuleId,
					FlightName = actionResponse.FlightName,
					ActionPath = actionResponse.ActionPath,
					Precedence = actionResponse.Precedence,
					Action = action
				};
			}
			catch (Exception ex)
			{
				throw new TargetedNotificationsException(ex.Message, ex);
			}
		}

		public static Dictionary<string, ITelemetryEventMatch> GetTriggers(this ActionResponse actionResponse)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(actionResponse.TriggerJson))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<Dictionary<string, ITelemetryEventMatch>>(actionResponse.TriggerJson, matchConvertersLazy.Value);
			}
			catch (Exception ex)
			{
				throw new TargetedNotificationsException(ex.Message, ex);
			}
		}

		public static Dictionary<string, ActionTriggerOptions> GetTriggerOptions(this ActionResponse actionResponse)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(actionResponse.TriggerJson))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<Dictionary<string, ActionTriggerOptions>>(actionResponse.TriggerJson);
			}
			catch (Exception ex)
			{
				throw new TargetedNotificationsException(ex.Message, ex);
			}
		}
	}
}
