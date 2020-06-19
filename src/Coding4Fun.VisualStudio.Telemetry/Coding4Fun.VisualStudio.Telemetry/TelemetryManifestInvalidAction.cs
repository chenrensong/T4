using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Invalid action is necessary for specify invalid (unrecognized) action on JSON deserialization stage.
	/// During rule validation stage this action should be ignored and removed from the action list.
	/// It allows us to partitially parse manifest files with new actions.
	/// </summary>
	internal class TelemetryManifestInvalidAction : ITelemetryManifestAction, IEventProcessorAction
	{
		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		public int Priority
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Execute action on event, using telemetryManifestContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false action forbids the event.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether current action is not explicitely forbid current event</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			throw new TelemetryManifestValidationException("invalid action item");
		}
	}
}
