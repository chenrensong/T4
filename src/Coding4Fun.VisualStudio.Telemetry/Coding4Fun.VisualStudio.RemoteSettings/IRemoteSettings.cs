using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Remote settings provide configurable settings without code changes.
	/// </summary>
	public interface IRemoteSettings : IDisposable
	{
		/// <summary>
		/// Subscribe to this event to be notified when Remote Settings have been updated.
		/// </summary>
		event EventHandler SettingsUpdated;

		/// <summary>
		/// Gets a remote setting value that is updated with both Targeted Notifications backend and RemoteControl file.
		/// This does not return the most up-to-date setting, but the value
		/// of whatever RemoteSettings has processed so far.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the remote setting</param>
		/// <param name="defaultValue">Value to return if remote setting does not exist</param>
		/// <returns>Remote setting value if it exists, otherwise defaultValue</returns>
		T GetValue<T>(string collectionPath, string key, T defaultValue);

		/// <summary>
		/// Gets remote setting value if one exists.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the Remote Setting</param>
		/// <param name="value">The value if it exists or default(T)</param>
		/// <returns>True if value exists, false if it does not</returns>
		bool TryGetValue<T>(string collectionPath, string key, out T value);

		/// <summary>
		/// Gets a remote setting value, that is updated with both Targeted Notifications backend and RemoteControl
		/// file. Must be called after Start.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the remote setting</param>
		/// <param name="defaultValue">Value to return if remote setting does not exist</param>
		/// <returns>Remote setting value if it exists, otherwise defaultValue</returns>
		Task<T> GetValueAsync<T>(string collectionPath, string key, T defaultValue);

		/// <summary>
		/// Gets kind of a remote setting value.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the remote setting</param>
		/// <returns>Kind of the value or unknown if it does not exist or error.</returns>
		ValueKind GetValueKind(string collectionPath, string key);

		/// <summary>
		/// Gets all remote actions of type T, wrapped in ActionWrapper. Waits for the call to Targeted Notifications backend
		/// to complete. Must be called after Start.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="actionPath">Unique path to identify the actions to retrieve</param>
		/// <returns></returns>
		Task<IEnumerable<ActionWrapper<T>>> GetActionsAsync<T>(string actionPath);

		/// <summary>
		/// Starts a background operation to check for new Remote Settings on both Targeted Notifictions and Remote Control
		/// backend and apply them.
		/// </summary>
		void Start();

		/// <summary>
		/// Add a scope filter provider. Not implemented yet.
		/// </summary>
		/// <param name="scopeFilterProvider">A filter provider</param>
		/// <returns>IRemoteSettings interface for chaining</returns>
		IRemoteSettings RegisterFilterProvider(IScopeFilterProvider scopeFilterProvider);

		/// <summary>
		/// Gets all the property names under a specific collection.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <returns>IEnumerable of all properties under the specified collection. Empty if no properties exist.</returns>
		IEnumerable<string> GetPropertyNames(string collectionPath);

		/// <summary>
		/// Gets all the sub-collection names under a specific collection.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <returns>IEnumerable of the names of the sub collections. Empty if it does not exist.</returns>
		IEnumerable<string> GetSubCollectionNames(string collectionPath);

		/// <summary>
		/// Determines if the collection exists.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <returns>True if the colleciton exists, otherwise false</returns>
		bool CollectionExists(string collectionPath);

		/// <summary>
		/// Determines if the property exists.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the Remote Setting</param>
		/// <returns>True if the property exists, otherwise false</returns>
		bool PropertyExists(string collectionPath, string key);
	}
}
