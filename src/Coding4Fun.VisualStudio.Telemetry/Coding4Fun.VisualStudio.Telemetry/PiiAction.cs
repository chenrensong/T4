using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class PiiAction : IEventProcessorAction, IEventProcessorActionDiagnostics
	{
		private const string UnknownValue = "Unknown";

		private readonly HashSet<string> piiedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private readonly IPiiPropertyProcessor piiPropertyProcessor;

		private int totalPiiProperties;

		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		public int Priority => 100;

		public PiiAction(IPiiPropertyProcessor piiPropertyProcessor)
		{
			CodeContract.RequiresArgumentNotNull<IPiiPropertyProcessor>(piiPropertyProcessor, "piiPropertyProcessor");
			this.piiPropertyProcessor = piiPropertyProcessor;
		}

		/// <summary>
		/// Execute action on event, using eventProcessorContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false action forbids the event.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether current action is not explicitely forbid current event</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			CodeContract.RequiresArgumentNotNull<IEventProcessorContext>(eventProcessorContext, "eventProcessorContext");
			TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
			List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
			foreach (KeyValuePair<string, object> property in telemetryEvent.Properties)
			{
				if (property.Value != null && property.Value.GetType() == piiPropertyProcessor.TypeOfPiiProperty())
				{
					list.Add(new KeyValuePair<string, object>(property.Key, piiPropertyProcessor.ConvertToHashedValue(property.Value)));
					if (piiPropertyProcessor.CanAddRawValue(eventProcessorContext))
					{
						list.Add(new KeyValuePair<string, object>(piiPropertyProcessor.BuildRawPropertyName(property.Key), piiPropertyProcessor.ConvertToRawValue(property.Value)));
					}
					piiedProperties.Add(property.Key);
					totalPiiProperties++;
				}
			}
			foreach (KeyValuePair<string, object> item in list)
			{
				telemetryEvent.Properties[item.Key] = item.Value;
			}
			return true;
		}

		public void PostDiagnosticInformation(TelemetrySession mainSession, TelemetryManifest newManifest)
		{
			if (totalPiiProperties != 0)
			{
				string value = "Unknown";
				TelemetryManifest currentManifest = mainSession.EventProcessor.CurrentManifest;
				if (currentManifest != null)
				{
					value = currentManifest.Version;
				}
				TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/PiiProperties");
				telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.Version"] = value;
				telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.HostName"] = mainSession.HostName;
				telemetryEvent.Properties["VS.TelemetryApi.PiiProperties.TotalCount"] = totalPiiProperties;
				telemetryEvent.Properties["VS.TelemetryApi.PiiProperties.Properties"] = StringExtensions.Join(piiedProperties.Select((string x) => x.ToLower(CultureInfo.InvariantCulture)), ",");
				mainSession.PostEvent(telemetryEvent);
			}
		}
	}
}
