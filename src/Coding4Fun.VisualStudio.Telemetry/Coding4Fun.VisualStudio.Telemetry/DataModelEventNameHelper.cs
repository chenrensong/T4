using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A helper class to validate if event name conforms to data model event name schema, and set properties based on the name.
	/// Here's the data model event name schema,
	/// It requires that event name is a unique, not null or empty string.
	/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
	/// For examples,
	/// vs/platform/opensolution;
	/// vs/platform/editor/lightbulb/fixerror;
	/// </summary>
	public static class DataModelEventNameHelper
	{
		private const char Separator = '/';

		/// <summary>
		/// Set product name, feature name and entity name by parsing event name string.
		/// </summary>
		/// <param name="operationEvent">Operation event</param>
		public static void SetProductFeatureEntityName(OperationEvent operationEvent)
		{
			SetProductFeatureEntityName(operationEvent.Name, operationEvent.ReservedProperties);
		}

		/// <summary>
		/// Set product name, feature name and entity name by parsing event name string.
		/// </summary>
		/// <param name="faultEvent">Fault event</param>
		public static void SetProductFeatureEntityName(FaultEvent faultEvent)
		{
			SetProductFeatureEntityName(faultEvent.Name, faultEvent.ReservedProperties);
		}

		/// <summary>
		/// Set product name, feature name and entity name by parsing event name string.
		/// </summary>
		/// <param name="assetEvent">Asset event</param>
		public static void SetProductFeatureEntityName(AssetEvent assetEvent)
		{
			SetProductFeatureEntityName(assetEvent.Name, assetEvent.ReservedProperties);
		}

		/// <summary>
		/// Set product name, feature name and entity name by parsing event name string.
		/// If the event name doesn't comply with data model event name schema, an exception will be thrown.
		/// Data model event name schema:
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </summary>
		/// <param name="eventName">event name</param>
		/// <param name="reservedProperties">the reserved properties dictionary from event used to set data model properties.</param>
		private static void SetProductFeatureEntityName(string eventName, IDictionary<string, object> reservedProperties)
		{
			ValidateEventName(eventName);
			int num = eventName.IndexOf('/');
			int num2 = eventName.LastIndexOf('/');
			reservedProperties["DataModel.ProductName"] = eventName.Substring(0, num);
			reservedProperties["DataModel.FeatureName"] = eventName.Substring(num + 1, num2 - num - 1);
			reservedProperties["DataModel.EntityName"] = eventName.Substring(num2 + 1);
		}

		/// <summary>
		/// Validate the event name complies with data model event name schema.
		/// </summary>
		/// <param name="eventName"></param>
		private static void ValidateEventName(string eventName)
		{
			int num = 0;
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(eventName, "eventName");
			if (eventName[0] == '/' || eventName[eventName.Length - 1] == '/')
			{
				throw new ArgumentException("Event name shouldn't start or end with slash.", "eventName");
			}
			for (int i = 0; i < eventName.Length; i++)
			{
				if (eventName[i] == ' ')
				{
					throw new ArgumentException("Event name shouldn't have whitespaces.", "eventName");
				}
				if (eventName[i] == '/')
				{
					num++;
					if (i > 0 && eventName[i - 1] == '/')
					{
						throw new ArgumentException("Event name shouldn't have successive slashes.", "eventName");
					}
				}
			}
			if (num < 2)
			{
				throw new ArgumentException("Event name should have at least 3 parts separated by slash.", "eventName");
			}
		}
	}
}
