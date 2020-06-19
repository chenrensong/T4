namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Interface that describes a means of locally storing a
	/// CachedTargetedNotifications object representing locally
	/// cached TN rules.
	/// </summary>
	internal interface ITargetedNotificationsCacheStorageProvider
	{
		/// <summary>
		/// Loads the cache and returns a copy of it
		/// </summary>
		/// <returns></returns>
		CachedTargetedNotifications GetLocalCacheCopy();

		/// <summary>
		/// Saves the cache, overriding any existing cache
		/// </summary>
		/// <param name="newCache"></param>
		void SetLocalCache(CachedTargetedNotifications newCache);

		/// <summary>
		/// Locks all access to cache across all threads
		/// and processes
		/// </summary>
		/// <param name="timeoutMs">Milliseconds to wait for the lock to be acquired before timing out. Leave null for infinite wait.</param>
		/// <returns>True if the lock was acquired. False if it was not (due to timeout)</returns>
		bool Lock(int? timeoutMs = null);

		/// <summary>
		/// Releases a held Lock
		/// </summary>
		void Unlock();

		/// <summary>
		/// Resets the local cache store to its initial/empty state
		/// </summary>
		void Reset();
	}
}
