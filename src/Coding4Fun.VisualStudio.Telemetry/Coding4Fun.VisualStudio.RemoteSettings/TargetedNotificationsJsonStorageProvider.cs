using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class TargetedNotificationsJsonStorageProvider : ITargetedNotificationsCacheStorageProvider
	{
		private readonly string cacheDirectory;

		private readonly string cacheFileFullPath;

		private const string cacheLockName = "Global\\55F58BAB-BDB9-47D5-B85E-B4D8234E8FAA";

		private const string cacheFileName = "targetnote_v1.json";

		private ITargetedNotificationsTelemetry telemetry;

		private Lazy<Mutex> cacheLock;

		public TargetedNotificationsJsonStorageProvider(RemoteSettingsInitializer initializer)
		{
			cacheDirectory = initializer.GetLocalAppDataRoot();
			cacheFileFullPath = Path.Combine(cacheDirectory, "targetnote_v1.json");
			telemetry = initializer.TargetedNotificationsTelemetry;
			cacheLock = new Lazy<Mutex>(delegate
			{
				try
				{
					return new Mutex(false, "Global\\55F58BAB-BDB9-47D5-B85E-B4D8234E8FAA");
				}
				catch (Exception exception)
				{
					string eventName = "VS/Core/TargetedNotifications/MutexFailure";
					telemetry.PostCriticalFault(eventName, "Failed to create Mutex", exception);
					return null;
				}
			});
		}

		/// <summary>
		/// Acquires a system-wide lock on the shared cache
		/// </summary>
		/// <param name="timeoutMs">Max milliseconds to wait for lock</param>
		/// <returns>True if the lock was acquired, False if the timeout was reached without acquiring the lock</returns>
		public bool Lock(int? timeoutMs = null)
		{
			try
			{
				if (cacheLock.Value == null)
				{
					return false;
				}
				if (timeoutMs.HasValue)
				{
					return cacheLock.Value.WaitOne(timeoutMs.Value);
				}
				return cacheLock.Value.WaitOne();
			}
			catch (AbandonedMutexException)
			{
				return true;
			}
			catch (Exception exception)
			{
				string eventName = "VS/Core/TargetedNotifications/MutexFailure";
				telemetry.PostCriticalFault(eventName, "Failed to lock Mutex", exception);
				return false;
			}
		}

		/// <summary>
		/// Releases a previously acquired system-wide lock on the shared cache
		/// </summary>
		public void Unlock()
		{
			try
			{
				if (cacheLock.Value != null)
				{
					cacheLock.Value.ReleaseMutex();
				}
			}
			catch (ApplicationException exception)
			{
				string eventName = "VS/Core/TargetedNotifications/CacheUnbalancedUnlock";
				telemetry.PostCriticalFault(eventName, "Unbalanced call to Unlock", exception);
			}
			catch (Exception exception2)
			{
				string eventName2 = "VS/Core/TargetedNotifications/MutexFailure";
				telemetry.PostCriticalFault(eventName2, "Failed to unlock Mutex", exception2);
			}
		}

		public void Reset()
		{
			try
			{
				File.Delete(cacheFileFullPath);
			}
			catch (Exception exception)
			{
				string eventName = "VS/Core/TargetedNotifications/CacheResetFailure";
				telemetry.PostCriticalFault(eventName, "Failed to reset the local cache", exception);
			}
		}

		public CachedTargetedNotifications GetLocalCacheCopy()
		{
			if (!File.Exists(cacheFileFullPath))
			{
				return new CachedTargetedNotifications();
			}
			string text;
			using (StreamReader streamReader = new StreamReader(cacheFileFullPath))
			{
				text = streamReader.ReadToEnd();
			}
			try
			{
				return JsonConvert.DeserializeObject<CachedTargetedNotifications>(text) ?? new CachedTargetedNotifications();
			}
			catch (Exception exception)
			{
				string eventName = "VS/Core/TargetedNotifications/CacheDeserializationFailure";
				telemetry.PostCriticalFault(eventName, "Failed to deserialize the local cache", exception);
				Reset();
				return new CachedTargetedNotifications();
			}
		}

		public void SetLocalCache(CachedTargetedNotifications newCache)
		{
			if (!Directory.Exists(cacheDirectory))
			{
				Directory.CreateDirectory(cacheDirectory);
			}
			using (StreamWriter streamWriter = new StreamWriter(cacheFileFullPath, false))
			{
				streamWriter.Write(JsonConvert.SerializeObject((object)newCache));
			}
		}
	}
}
