using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	internal class VSTelemetryEvent : ITelemetryEvent
	{
		private readonly OperationEvent endEvent;

		private readonly DataPointCollection sharedProperties;

		public double? Duration => endEvent.Duration;

		public IEnumerable<DataPoint> SharedProperties => sharedProperties;

		public VSTelemetryEvent(OperationEvent operationEvent)
		{
			endEvent = (operationEvent ?? throw new ArgumentNullException("operationEvent"));
			sharedProperties = new DataPointCollection();
		}

		public void SetProperty(TelemetryIdentifier propertyIdentifier, object propertyValue, bool shared = false, bool pii = false)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (propertyIdentifier == null)
			{
				throw new ArgumentNullException("propertyIdentifier");
			}
			object value = pii ? ((object)new TelemetryPiiProperty(propertyValue)) : propertyValue;
			if (shared)
			{
				sharedProperties.Add(propertyIdentifier, value);
			}
			else
			{
				((TelemetryEvent)endEvent).Properties[propertyIdentifier.Value] = value;
			}
		}

		public void SetProperties(IEnumerable<DataPoint> properties, bool shared = false)
		{
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}
			if (shared)
			{
				sharedProperties.AddRange(properties);
			}
			else
			{
				DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)endEvent).Properties);
			}
		}

		public void Correlate(params TelemetryEventCorrelation[] correlations)
		{
			((TelemetryEvent)endEvent).Correlate(correlations);
		}
	}
}
