using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Base class for all manifest OptOut actions
	/// </summary>
	internal abstract class TelemetryManifestActionOptOutBase : ITelemetryManifestAction, IEventProcessorAction
	{
		[JsonIgnore]
		public int Priority => 2;

		/// <summary>
		/// Default Execute method checks if session is OptedOut and call specific method
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns></returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			if (!eventProcessorContext.HostTelemetrySession.IsOptedIn)
			{
				ExecuteOptOutAction(eventProcessorContext);
			}
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
		protected abstract void ExecuteOptOutAction(IEventProcessorContext eventProcessorContext);
	}
}
