using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Explicitily exclude properties from the matched events from the OptOut session,
	/// even if these properties were included by the custom OptOut action
	/// </summary>
	internal class TelemetryManifestActionOptOutExcludeProperties : TelemetryManifestActionOptOutPropertiesBase
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<string> Properties
		{
			get
			{
				return base.PropertiesImpl;
			}
			set
			{
				CodeContract.RequiresArgumentNotNull<IEnumerable<string>>(value, "value");
				base.PropertiesImpl = value;
			}
		}

		/// <summary>
		/// Implementation of the ProcessPropertyName
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="eventProcessorContext"></param>
		protected override void ProcessPropertyName(string propertyName, IEventProcessorContext eventProcessorContext)
		{
			eventProcessorContext.ExcludePropertyFromEvent(propertyName);
		}
	}
}
