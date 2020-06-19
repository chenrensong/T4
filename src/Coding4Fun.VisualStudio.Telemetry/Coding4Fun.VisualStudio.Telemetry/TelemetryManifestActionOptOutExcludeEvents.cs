namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Explicitily exclude matched events from the OptOut session, even if these events were
	/// included by the custom OptOut action
	/// </summary>
	internal class TelemetryManifestActionOptOutExcludeEvents : TelemetryManifestActionOptOutBase
	{
		/// <summary>
		/// Implementation of the TelemetryManifestActionOptOutBase.ExecuteOptOutAction method
		/// Logic is very simple. If event matches current conditions then it is not valid for the OptOut session
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		protected override void ExecuteOptOutAction(IEventProcessorContext eventProcessorContext)
		{
			eventProcessorContext.IsEventDropped = true;
		}
	}
}
