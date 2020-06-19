using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// OptOut default action.
	/// This action is executed only when session is OptOut.
	/// In this case it checks if event name is in the list of the OptOut friendly event name.
	/// If not IsEventDropped flag is set.
	/// Also it checks every property. If property name is not in the OptOut friendly property name
	/// property is removed from the event and placed to the excluded list.
	/// </summary>
	internal sealed class OptOutAction : IEventProcessorAction
	{
		private readonly HashSet<string> optoutFriendlyEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private readonly HashSet<string> optoutFriendlyProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		public int Priority => 1;

		/// <summary>
		/// Execute action on event, using telemetryManifestContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false action forbids the event.
		/// By default OptOut set IsEventDropped to the true, and exclude all properties.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether current action is not explicitely forbid current event</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			if (!eventProcessorContext.HostTelemetrySession.IsOptedIn)
			{
				TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
				if (!telemetryEvent.IsOptOutFriendly && !optoutFriendlyEvents.Contains(telemetryEvent.Name))
				{
					eventProcessorContext.IsEventDropped = true;
				}
				foreach (string item in new List<string>(telemetryEvent.Properties.Keys))
				{
					if ((!telemetryEvent.IsOptOutFriendly || TelemetryEvent.IsPropertyNameReserved(item) || TelemetryContext.IsPropertyNameReserved(item)) && !optoutFriendlyProperties.Contains(item))
					{
						eventProcessorContext.ExcludePropertyFromEvent(item);
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Add OptOut friendly event name
		/// </summary>
		/// <param name="eventName"></param>
		public void AddOptOutFriendlyEventName(string eventName)
		{
			CodeContract.RequiresArgumentNotNull<string>(eventName, "eventName");
			optoutFriendlyEvents.Add(eventName);
		}

		/// <summary>
		/// Add OptOut friendly properties to the action
		/// </summary>
		/// <param name="propertyNameList"></param>
		public void AddOptOutFriendlyPropertiesList(IEnumerable<string> propertyNameList)
		{
			CodeContract.RequiresArgumentNotNull<IEnumerable<string>>(propertyNameList, "propertyNameList");
			optoutFriendlyProperties.UnionWith(propertyNameList);
		}
	}
}
