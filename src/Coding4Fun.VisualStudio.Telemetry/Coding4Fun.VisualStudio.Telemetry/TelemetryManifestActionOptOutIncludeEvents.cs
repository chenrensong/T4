namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Explicitily include matched events from the OptOut session
	/// </summary>
	internal class TelemetryManifestActionOptOutIncludeEvents : TelemetryManifestActionOptOutBase
	{
		/// <summary>
		/// Implementation of the TelemetryManifestActionOptOutBase.ExecuteOptOutAction method
		/// Logic is very simple. If event matches current conditions then it is valid for the OptOut session
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		protected override void ExecuteOptOutAction(IEventProcessorContext eventProcessorContext)
		{
			eventProcessorContext.IsEventDropped = false;
		}
	}
}
