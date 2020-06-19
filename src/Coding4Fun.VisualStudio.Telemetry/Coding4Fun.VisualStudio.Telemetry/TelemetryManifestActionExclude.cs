using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestActionExclude : ITelemetryManifestAction, IEventProcessorAction
	{
		/// <summary>
		/// Calculated field on a deserialization step
		/// Channel id is equal to '*' make it true
		/// </summary>
		[JsonIgnore]
		private bool excludeAll;

		[JsonIgnore]
		private IEnumerable<string> channelsToExclude;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<string> ExcludeForChannels
		{
			get
			{
				return channelsToExclude;
			}
			set
			{
				CodeContract.RequiresArgumentNotNull<IEnumerable<string>>(value, "value");
				channelsToExclude = value;
				excludeAll = (value.Count() == 1 && value.First() == "*");
			}
		}

		/// <summary>
		/// Gets priority which is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		[JsonIgnore]
		public int Priority => 0;

		/// <summary>
		/// Execute action on event, using telemetryManifestContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false if it is last action to execute.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether is it allowed to the keep executing next actions</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			if (excludeAll)
			{
				return false;
			}
			CodeContract.RequiresArgumentNotNull<IEventProcessorContext>(eventProcessorContext, "eventProcessorContext");
			foreach (string excludeForChannel in ExcludeForChannels)
			{
				eventProcessorContext.Router.DisableChannel(excludeForChannel);
			}
			return true;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public void Validate()
		{
			if (ExcludeForChannels == null)
			{
				throw new TelemetryManifestValidationException("excludeForChannels must not be null");
			}
			int num = ExcludeForChannels.Count();
			if (num == 0)
			{
				throw new TelemetryManifestValidationException("excludeForChannels must have at least one channel");
			}
			foreach (string excludeForChannel in ExcludeForChannels)
			{
				if (string.IsNullOrEmpty(excludeForChannel))
				{
					throw new TelemetryManifestValidationException("excludeForChannels must not contain empty/null channels");
				}
				if (excludeForChannel == "*" && num > 1)
				{
					throw new TelemetryManifestValidationException("excludeForChannels can contain only one element if '*' is in array");
				}
			}
		}
	}
}
