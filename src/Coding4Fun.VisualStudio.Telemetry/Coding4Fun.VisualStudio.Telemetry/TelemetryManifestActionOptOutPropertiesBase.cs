using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Base class for the work with the OptOut properties
	/// </summary>
	internal abstract class TelemetryManifestActionOptOutPropertiesBase : TelemetryManifestActionOptOutBase
	{
		/// <summary>
		/// Gets or sets the properties used by the action.
		/// Note: derived classes should never set null to the property.
		/// </summary>
		[JsonIgnore]
		protected IEnumerable<string> PropertiesImpl
		{
			get;
			set;
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public override void Validate()
		{
			if (!PropertiesImpl.Any())
			{
				throw new TelemetryManifestValidationException("properties must have at least one property");
			}
			if (PropertiesImpl.Any(string.IsNullOrEmpty))
			{
				throw new TelemetryManifestValidationException("properties must not contain empty/null property name");
			}
		}

		/// <summary>
		/// Implementation of the TelemetryManifestActionOptOutBase.ExecuteOptOutAction method
		/// We need to include/exclude matched properties from/to event when user is OptedOut
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		protected override void ExecuteOptOutAction(IEventProcessorContext eventProcessorContext)
		{
			foreach (string item in PropertiesImpl)
			{
				ProcessPropertyName(item, eventProcessorContext);
			}
		}

		protected abstract void ProcessPropertyName(string propertyName, IEventProcessorContext eventProcessorContext);
	}
}
