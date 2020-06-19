using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal abstract class DataModelPropertyAction<T> : IEventProcessorAction, IEventProcessorActionDiagnostics where T : TelemetryDataModelProperty
	{
		private readonly HashSet<string> properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private const string UnknownValue = "Unknown";

		private int totalPropertyCount;

		private string flagPropertyName;

		public int Priority
		{
			get;
		}

		public string SuffixName
		{
			get;
		}

		public string DiagnosticName
		{
			get;
		}

		/// <summary>
		/// Execute action on event, using eventProcessorContext as a provider of the necessary information.
		/// This action add a suffix to property name which property is type T in the event.
		/// </summary>
		/// <param name="priority"></param>
		/// <param name="suffixName"></param>
		/// <param name="flagName"></param>
		/// <param name="diagnosticName"></param>
		public DataModelPropertyAction(int priority, string suffixName, string flagName, string diagnosticName)
		{
			Priority = priority;
			SuffixName = suffixName;
			DiagnosticName = diagnosticName;
			flagPropertyName = "Reserved.DataModel." + flagName;
		}

		/// <summary>
		/// Execute action on event, using eventProcessorContext as a provider of the necessary information.
		/// This action add a suffix DataModelSetting to all Setting property names in the event.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>
		/// Indicator, whether current action is not explicitely forbid current event.
		/// Return true if it is allowed to execute next actions.
		/// Return false action forbids the event.
		/// </returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			CodeContract.RequiresArgumentNotNull<IEventProcessorContext>(eventProcessorContext, "eventProcessorContext");
			TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
			KeyValuePair<string, T>[] array = (from prop in telemetryEvent.Properties
				where prop.Value is T
				select new KeyValuePair<string, T>(prop.Key, prop.Value as T)).ToArray();
			totalPropertyCount += array.Length;
			if (array.Any())
			{
				telemetryEvent.Properties[flagPropertyName] = true;
			}
			KeyValuePair<string, T>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<string, T> keyValuePair = array2[i];
				telemetryEvent.Properties[keyValuePair.Key + SuffixName] = keyValuePair.Value.Value;
				telemetryEvent.Properties.Remove(keyValuePair.Key);
				properties.Add(keyValuePair.Key);
			}
			return true;
		}

		public void PostDiagnosticInformation(TelemetrySession mainSession, TelemetryManifest newManifest)
		{
			if (totalPropertyCount != 0)
			{
				string value = "Unknown";
				TelemetryManifest currentManifest = mainSession.EventProcessor.CurrentManifest;
				if (currentManifest != null)
				{
					value = currentManifest.Version;
				}
				TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/" + DiagnosticName);
				telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.Version"] = value;
				telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.HostName"] = mainSession.HostName;
				telemetryEvent.Properties["VS.TelemetryApi." + DiagnosticName + ".TotalCount"] = totalPropertyCount;
				telemetryEvent.Properties["VS.TelemetryApi." + DiagnosticName + ".Properties"] = StringExtensions.Join(properties.Select((string x) => x.ToLower(CultureInfo.InvariantCulture)), ",");
				mainSession.PostEvent(telemetryEvent);
			}
		}
	}
}
