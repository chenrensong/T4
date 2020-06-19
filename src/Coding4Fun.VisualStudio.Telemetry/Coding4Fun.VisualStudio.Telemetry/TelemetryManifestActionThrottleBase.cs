using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Base class for all manifest OptOut actions
	/// </summary>
	internal abstract class TelemetryManifestActionThrottleBase : ITelemetryManifestAction, IEventProcessorAction
	{
		[JsonIgnore]
		public int Priority => 3;

		/// <summary>
		/// Default Execute method checks if session is OptedOut and call specific method
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns></returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			ExecuteThrottlingAction(eventProcessorContext);
			return true;
		}

		/// <summary>
		/// By default no validation needed
		/// </summary>
		public virtual void Validate()
		{
		}

		/// <summary>
		/// Each deriver should implement this specific OptOut action
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		protected abstract void ExecuteThrottlingAction(IEventProcessorContext eventProcessorContext);
	}
}
