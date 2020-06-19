using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Suppress all context/postproperty events, which doesn't contain useful payload.
	/// For example, when properties were removed by other actions it doesn't make sense to post
	/// this event with default reserved and shared context properties.
	/// </summary>
	internal sealed class SuppressEmptyPostPropertyEventAction : IEventProcessorAction
	{
		public int Priority => 300;

		/// <summary>
		/// Validate coming event
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns></returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
			if (TelemetryContext.IsEventNameContextPostProperty(telemetryEvent.Name))
			{
				return telemetryEvent.Properties.Any((KeyValuePair<string, object> p) => !TelemetryEvent.IsPropertyNameReserved(p.Key) && !TelemetryContext.IsPropertyNameReserved(p.Key));
			}
			return true;
		}
	}
}
