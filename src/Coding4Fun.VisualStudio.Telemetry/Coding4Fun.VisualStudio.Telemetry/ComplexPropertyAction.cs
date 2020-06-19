using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class ComplexPropertyAction : IEventProcessorAction
	{
		internal const int MaxSerializedLength = 61440;

		internal const string FailedToSerializePropertyName = "Reserved.ComplexProperty.FailedToSerialize";

		internal const string TruncatedPropertyName = "Reserved.ComplexProperty.Truncated";

		/// <summary>
		/// Use factory here to not load additional modules for serializer before session is Started.
		/// </summary>
		private readonly IComplexObjectSerializerFactory serializerFactory;

		private readonly IPiiPropertyProcessor piiProcessor;

		private readonly Func<object, string> converterToHashValue;

		private readonly Func<object, string> converterToRawValue;

		private IComplexObjectSerializer serializer;

		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		public int Priority => 250;

		public ComplexPropertyAction(IComplexObjectSerializerFactory serializerFactory, IPiiPropertyProcessor piiProcessor)
		{
			CodeContract.RequiresArgumentNotNull<IComplexObjectSerializerFactory>(serializerFactory, "serializerFactory");
			CodeContract.RequiresArgumentNotNull<IPiiPropertyProcessor>(piiProcessor, "piiProcessor");
			this.serializerFactory = serializerFactory;
			this.piiProcessor = piiProcessor;
			converterToHashValue = ((object value) => this.piiProcessor.ConvertToHashedValue(value));
			converterToRawValue = ((object value) => this.piiProcessor.ConvertToRawValue(value).ToString());
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
			EnsureSerializerIsInitialized();
			TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, object> property in telemetryEvent.Properties)
			{
				if (property.Value is TelemetryComplexProperty)
				{
					try
					{
						SerializeProperty(property.Key, property.Value as TelemetryComplexProperty, eventProcessorContext, list);
					}
					catch (ComplexObjectSerializerException ex)
					{
						dictionary.Add(property.Key, ex.Message);
					}
				}
			}
			if (dictionary.Count > 0)
			{
				try
				{
					SerializeProperty("Reserved.ComplexProperty.FailedToSerialize", new TelemetryComplexProperty(dictionary), eventProcessorContext, list);
				}
				catch
				{
				}
			}
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				telemetryEvent.Properties.Remove(item.Key);
			}
			HashSet<string> hashSet = new HashSet<string>();
			foreach (KeyValuePair<string, string> item2 in list)
			{
				if (item2.Value.Length <= 61440)
				{
					telemetryEvent.Properties[item2.Key] = item2.Value;
				}
				else
				{
					telemetryEvent.Properties[item2.Key] = item2.Value.Substring(0, 61437) + "...";
					hashSet.Add(item2.Key);
				}
			}
			if (hashSet.Count > 0)
			{
				list.Clear();
				try
				{
					SerializeProperty("Reserved.ComplexProperty.Truncated", new TelemetryComplexProperty(hashSet), eventProcessorContext, list);
				}
				catch
				{
				}
				foreach (KeyValuePair<string, string> item3 in list)
				{
					telemetryEvent.Properties[item3.Key] = item3.Value;
				}
			}
			return true;
		}

		private void EnsureSerializerIsInitialized()
		{
			if (serializer == null)
			{
				serializer = serializerFactory.Instance();
			}
		}

		private void SerializeProperty(string propertyName, TelemetryComplexProperty propertyValue, IEventProcessorContext eventProcessorContext, List<KeyValuePair<string, string>> propertiesToModify)
		{
			serializer.SetTypeConverter(piiProcessor.TypeOfPiiProperty(), converterToHashValue);
			propertiesToModify.Add(new KeyValuePair<string, string>(propertyName, serializer.Serialize(propertyValue.Value)));
			if (serializer.WasConverterUsedForType(piiProcessor.TypeOfPiiProperty()) && piiProcessor.CanAddRawValue(eventProcessorContext))
			{
				serializer.SetTypeConverter(piiProcessor.TypeOfPiiProperty(), converterToRawValue);
				propertiesToModify.Add(new KeyValuePair<string, string>(piiProcessor.BuildRawPropertyName(propertyName), serializer.Serialize(propertyValue.Value)));
			}
		}
	}
}
