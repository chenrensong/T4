using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchPropertyValue : ITelemetryManifestMatch, ITelemetryEventMatch
	{
		[JsonIgnore]
		private string propertyName;

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string Property
		{
			get
			{
				return propertyName;
			}
			set
			{
				CodeContract.RequiresArgumentNotNullAndNotEmpty(value, "value");
				propertyName = value.ToLower(CultureInfo.InvariantCulture);
			}
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public ITelemetryManifestMatchValue Value
		{
			get;
			set;
		}

		public IEnumerable<ITelemetryManifestMatch> GetChildren()
		{
			return Enumerable.Empty<ITelemetryManifestMatch>();
		}

		/// <summary>
		/// Check, whether does event match conditions
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <returns></returns>
		public bool IsEventMatch(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			if (!telemetryEvent.Properties.TryGetValue(Property, out object value))
			{
				return false;
			}
			return Value.IsMatch(value);
		}

		/// <summary>
		/// Validates only 'this' class but not children. Explicity specify interface so that this method
		/// can only be called an instance typed to the interface and not an implementation type.
		/// </summary>
		void ITelemetryManifestMatch.ValidateItself()
		{
			if (StringExtensions.IsNullOrWhiteSpace(Property))
			{
				throw new TelemetryManifestValidationException("'property' can't be empty");
			}
			if (Value == null)
			{
				throw new TelemetryManifestValidationException("'value' can't be null");
			}
			Value.Validate();
		}
	}
}
