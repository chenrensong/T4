namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// IEventProcessorAction is used by the EventProcessor for the process events
	/// </summary>
	internal interface IEventProcessorAction
	{
		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		int Priority
		{
			get;
		}

		/// <summary>
		/// Execute action on event, using telemetryManifestContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false action forbids the event.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether current action is not explicitely forbid current event</returns>
		bool Execute(IEventProcessorContext eventProcessorContext);
	}
}
