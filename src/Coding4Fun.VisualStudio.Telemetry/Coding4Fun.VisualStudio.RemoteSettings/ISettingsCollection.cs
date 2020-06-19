using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface ISettingsCollection
	{
		/// <summary>
		/// Gets kind of value from storage.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <param name="key"></param>
		/// <param name="kind"></param>
		/// <returns></returns>
		bool TryGetValueKind(string collectionPath, string key, out ValueKind kind);

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
		/// Determines if the property exists.
		/// </summary>
		/// <param name="collectionPath"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		bool PropertyExists(string collectionPath, string propertyName);
	}
}
