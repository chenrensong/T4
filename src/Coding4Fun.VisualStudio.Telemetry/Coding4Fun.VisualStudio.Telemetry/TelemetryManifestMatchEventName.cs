using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchEventName : ITelemetryManifestMatch, ITelemetryEventMatch
	{
		[JsonIgnore]
		private TelemetryEventMatchByName eventMatchFilter;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string EventName
		{
			get
			{
				if (eventMatchFilter == null)
				{
					return null;
				}
				if (!eventMatchFilter.IsFullNameCheck)
				{
					return eventMatchFilter.EventName + "*";
				}
				return eventMatchFilter.EventName;
			}
			set
			{
				CodeContract.RequiresArgumentNotNullAndNotEmpty(value, "value");
				string text = value.ToLower(CultureInfo.InvariantCulture);
				if (text.EndsWith("*", StringComparison.Ordinal))
				{
					eventMatchFilter = new TelemetryEventMatchByName(text.Remove(text.Length - 1), false);
				}
				else
				{
					eventMatchFilter = new TelemetryEventMatchByName(text, true);
				}
			}
		}

		/// <summary>
		/// Check, whether does event match conditions
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <returns></returns>
		public bool IsEventMatch(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			return eventMatchFilter.IsEventMatch(telemetryEvent);
		}

		public IEnumerable<ITelemetryManifestMatch> GetChildren()
		{
			return Enumerable.Empty<ITelemetryManifestMatch>();
		}

		/// <summary>
		/// Validates only 'this' class but not children. Explicity specify interface so that this method
		/// can only be called an instance typed to the interface and not an implementation type.
		/// </summary>
		void ITelemetryManifestMatch.ValidateItself()
		{
			if (string.IsNullOrEmpty(EventName))
			{
				throw new TelemetryManifestValidationException("'event' name can't be null");
			}
		}
	}
}
