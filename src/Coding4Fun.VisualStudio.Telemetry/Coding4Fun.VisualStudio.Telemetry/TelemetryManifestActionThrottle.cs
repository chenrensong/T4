namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestActionThrottle : TelemetryManifestActionThrottleBase
	{
		protected override void ExecuteThrottlingAction(IEventProcessorContext eventProcessorContext)
		{
			eventProcessorContext.ThrottlingAction = ThrottlingAction.Throttle;
		}
	}
}
