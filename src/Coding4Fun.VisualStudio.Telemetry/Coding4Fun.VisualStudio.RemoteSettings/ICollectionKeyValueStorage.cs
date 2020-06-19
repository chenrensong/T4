using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Storage which provides key-value pairs, along with collections of key-value pairs.
	/// </summary>
	public interface ICollectionKeyValueStorage
	{
		/// <summary>
		/// Gets current value from the storage
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath"></param>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		T GetValue<T>(string collectionPath, string key, T defaultValue);

		/// <summary>
		/// Gets current value from storage and indicates if key was found in storage.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns>true if value in storage, default(T) if not in storage</returns>
		bool TryGetValue<T>(string collectionPath, string key, out T value);

		/// <summary>
		/// Gets kind of value from storage.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <param name="key"></param>
		/// <param name="kind"></param>
		/// <returns></returns>
		bool TryGetValueKind(string collectionPath, string key, out ValueKind kind);

		/// <summary>
		/// Sets value to the storage.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void SetValue<T>(string collectionPath, string key, T value);

		/// <summary>
		/// Gets all the property names under a specific collection.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <returns></returns>
		IEnumerable<string> GetPropertyNames(string collectionPath);

		/// <summary>
		/// Gets all the sub-collection names under a specific collection.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <returns></returns>
		IEnumerable<string> GetSubCollectionNames(string collectionPath);

		/// <summary>
		/// Determines if the collection exists.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <returns></returns>
		bool CollectionExists(string collectionPath);

		/// <summary>
		/// Determines if the collection exists.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		bool PropertyExists(string collectionPath, string propertyName);

		/// <summary>
		/// Deletes an entire collection, it's properties, and all sub-collections.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <returns></returns>
		bool DeleteCollection(string collectionPath);

		/// <summary>
		/// Deletes a property under a collection.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		bool DeleteProperty(string collectionPath, string propertyName);
	}
}
