using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Represents a property bag that survives process shutdown.
	/// </summary>
	internal interface IPersistentPropertyBag
	{
		/// <summary>Persists any changes</summary>
		void Persist();

		/// <summary>
		/// Sets a new integer property value. The latest property value will be stored.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		void SetProperty(string propertyName, int value);

		/// <summary>
		/// Sets a new string property value. The latest property value will be stored.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		void SetProperty(string propertyName, string value);

		/// <summary>
		/// Sets a new object property value. The latest property value will be stored.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		void SetProperty(string propertyName, double value);

		/// <summary>
		/// Gets a specific property value.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		object GetProperty(string propertyName);

		/// <summary>
		/// Removes a specific property value.
		/// </summary>
		/// <param name="propertyName"></param>
		void RemoveProperty(string propertyName);

		/// <summary>
		/// Gets all properties with undefined order.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<string, object>> GetAllProperties();

		/// <summary>
		/// Removes all properties from persistent property bag.
		/// </summary>
		void Clear();
	}
}
