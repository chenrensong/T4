using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	/// <summary>
	/// DataPointCollection - Represents a collection of DataPoints to report to Visual Studio telemetry servers
	/// </summary>
	public class DataPointCollection : List<DataPoint>
	{
		public DataPointCollection()
		{
		}

		public DataPointCollection(int capacity)
			: base(capacity)
		{
		}

		public void Add(TelemetryIdentifier identifier, object value)
		{
			Add(new DataPoint(identifier, value));
		}

		/// <summary>
		/// Set collection items in dictionary
		/// </summary>
		/// <param name="dictionary">Dictionary to store items</param>
		/// <param name="collection">Collection of DataPoints to set</param>
		public static void AddCollectionToDictionary(IEnumerable<DataPoint> collection, IDictionary<string, object> dictionary)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			if (dictionary == null)
			{
				throw new ArgumentNullException("dictionary");
			}
			foreach (DataPoint item in collection)
			{
				dictionary[item.Identity.Value] = item.Value;
			}
		}
	}
}
