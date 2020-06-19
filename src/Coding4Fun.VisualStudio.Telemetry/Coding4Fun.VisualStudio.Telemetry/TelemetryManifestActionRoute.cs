using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestActionRoute : ITelemetryManifestAction, IEventProcessorAction
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<TelemetryManifestRouter> Route
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
		public int Priority => 10000;

		/// <summary>
		/// Execute action on event, using telemetryManifestContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false if it is last action to execute.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether is it allowed to the keep executing next actions</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			CodeContract.RequiresArgumentNotNull<IEventProcessorContext>(eventProcessorContext, "eventProcessorContext");
			foreach (TelemetryManifestRouter item in Route)
			{
				eventProcessorContext.Router.TryAddRouteArgument(item.ChannelId, item.Args);
			}
			return true;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (Route == null)
			{
				throw new TelemetryManifestValidationException("'route' is null");
			}
			if (!Route.Any())
			{
				throw new TelemetryManifestValidationException("'route' action must contain at least one element");
			}
			foreach (TelemetryManifestRouter item in Route)
			{
				item.Validate();
			}
		}
	}
}
