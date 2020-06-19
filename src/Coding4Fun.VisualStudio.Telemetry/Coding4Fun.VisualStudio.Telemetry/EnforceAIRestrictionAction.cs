using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This action enforce AppInsight restriction on the property.
	/// Action is check property name size/characters.
	/// Also it checks property value size.
	/// In case the property is not pass validation it is removed from the event and
	/// added to the special reserved property.
	/// </summary>
	internal sealed class EnforceAIRestrictionAction : IEventProcessorAction
	{
		private const int MaxPropertyNameLength = 150;

		private const int MaxPropertyValueLength = 1024;

		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		public int Priority => 200;

		/// <summary>
		/// Execute action on event, using telemetryManifestContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false action forbids the event.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether current action is not explicitely forbid current event</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			List<string> list = null;
			List<Tuple<string, string>> list2 = null;
			TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
			foreach (KeyValuePair<string, object> property in telemetryEvent.Properties)
			{
				if (!IsPropertyNameValid(property.Key))
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(property.Key);
				}
				else if (property.Value != null && !(property.Value is TelemetryComplexProperty))
				{
					string text = property.Value.ToString();
					if (!IsPropertyValueValid(text))
					{
						if (list2 == null)
						{
							list2 = new List<Tuple<string, string>>();
						}
						list2.Add(Tuple.Create(property.Key, text));
					}
				}
			}
			if (list != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string item in list)
				{
					telemetryEvent.Properties.Remove(item);
					if (stringBuilder.Length + 1 + item.Length <= 1024)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ",{0}", new object[1]
							{
								item
							});
						}
						else
						{
							stringBuilder.Append(item);
						}
					}
					else if (stringBuilder.Length == 0)
					{
						stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}...", new object[1]
						{
							item.Substring(0, 147)
						});
					}
				}
				telemetryEvent.Properties["Reserved.InvalidEvent.InvalidPropertyNames"] = stringBuilder.ToString();
			}
			if (list2 != null)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				foreach (Tuple<string, string> item2 in list2)
				{
					telemetryEvent.Properties[item2.Item1] = string.Format(CultureInfo.InvariantCulture, "{0}...", new object[1]
					{
						item2.Item2.Substring(0, 1021)
					});
					if (stringBuilder2.Length + 1 + item2.Item1.Length <= 150)
					{
						if (stringBuilder2.Length > 0)
						{
							stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, ",{0}", new object[1]
							{
								item2.Item1
							});
						}
						else
						{
							stringBuilder2.Append(item2.Item1);
						}
					}
				}
				telemetryEvent.Properties["Reserved.InvalidEvent.TruncatedProperties"] = stringBuilder2.ToString();
			}
			return true;
		}

		/// <summary>
		/// Check, whether text representation of the value is follow restriction
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private bool IsPropertyValueValid(string value)
		{
			return value.Length <= 1024;
		}

		/// <summary>
		/// Check, whether property name is follow restriction
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		private bool IsPropertyNameValid(string propertyName)
		{
			if (propertyName.Length > 150)
			{
				return false;
			}
			return propertyName.All((char c) => char.IsLetterOrDigit(c) || c == '_' || c == '/' || c == '\\' || c == '.' || c == '-');
		}
	}
}
