using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestActionPii : ITelemetryManifestAction, IEventProcessorAction
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<string> Properties
		{
			get;
			set;
		}

		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		[JsonIgnore]
		public int Priority => 4;

		/// <summary>
		/// Execute action on event, using eventProcessorContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false if it is last action to execute.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether is it allowed to the keep executing next actions</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			CodeContract.RequiresArgumentNotNull<IEventProcessorContext>(eventProcessorContext, "eventProcessorContext");
			TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
			foreach (string property in Properties)
			{
				if (telemetryEvent.Properties.TryGetValue(property, out object value) && !(value is TelemetryPiiProperty))
				{
					telemetryEvent.Properties[property] = new TelemetryPiiProperty(value);
				}
			}
			return true;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (Properties == null)
			{
				throw new TelemetryManifestValidationException("'properties' is null");
			}
			if (!Properties.Any())
			{
				throw new TelemetryManifestValidationException("'properties' action must contain at least one element");
			}
			if (Properties.Any((Func<string, bool>)StringExtensions.IsNullOrWhiteSpace))
			{
				throw new TelemetryManifestValidationException("an entry in 'properties' cannot be null or whitespace");
			}
		}
	}
}
