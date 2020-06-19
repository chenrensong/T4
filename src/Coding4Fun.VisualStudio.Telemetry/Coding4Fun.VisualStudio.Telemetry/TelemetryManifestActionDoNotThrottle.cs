namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestActionDoNotThrottle : TelemetryManifestActionThrottleBase
	{
		protected override void ExecuteThrottlingAction(IEventProcessorContext eventProcessorContext)
		{
			eventProcessorContext.ThrottlingAction = ThrottlingAction.DoNotThrottle;
		}
	}
}
