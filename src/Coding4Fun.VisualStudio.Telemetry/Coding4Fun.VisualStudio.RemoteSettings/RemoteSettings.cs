using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Remote settings provide configurable settings without code changes.
	/// </summary>
	public class RemoteSettings : TelemetryDisposableObject, IRemoteSettings, IDisposable, IRemoteSettings2
	{
		private static readonly Lazy<RemoteSettings> defaultRemoteSettings = new Lazy<RemoteSettings>(() => new RemoteSettings(RemoteSettingsInitializer.BuildDefault()));

		private readonly IStableRemoteSettingsProvider stableRemoteSettingProvider;

		private readonly List<IRemoteSettingsProvider> remoteSettingsProviders;

		private readonly IScopeParserFactory scopeParserFactory;

		private readonly IRemoteSettingsStorageHandler nonScopedStorageHandler;

		private readonly Func<bool> isUpdateDisabled;

		private readonly IRemoteSettingsLogger logger;

		private bool isStarted;

		internal Task StartTask;

		internal IEnumerable<IRemoteSettingsProvider> AllRemoteSettingsProviders
		{
			get
			{
				yield return stableRemoteSettingProvider;
				foreach (IRemoteSettingsProvider remoteSettingsProvider in remoteSettingsProviders)
				{
					yield return remoteSettingsProvider;
				}
			}
		}

		/// <summary>
		/// Gets a default remote settings instance that uses a "Default.json" file.
		/// </summary>
		public static IRemoteSettings Default => defaultRemoteSettings.Value;

		/// <summary>
		/// Subscribe to this event to be notified when Remote Settings have been updated.
		/// </summary>
		public event EventHandler SettingsUpdated;

		/// <summary>
		/// Construct a new Remote Setting instance with values taken from the initializer.
		/// </summary>
		/// <param name="initializer">Values with which to initialize</param>
		public RemoteSettings(RemoteSettingsInitializer initializer)
		{
			initializer.FillWithDefaults();
			scopeParserFactory = initializer.ScopeParserFactory;
			nonScopedStorageHandler = initializer.NonScopedRemoteSettingsStorageHandler;
			foreach (IScopeFilterProvider scopeFilterProvider in initializer.ScopeFilterProviders)
			{
				RegisterFilterProvider(scopeFilterProvider);
			}
			remoteSettingsProviders = initializer.RemoteSettingsProviders.Select((Func<RemoteSettingsInitializer, IRemoteSettingsProvider> x) => x(initializer)).ToList();
			stableRemoteSettingProvider = initializer.StableRemoteSettingsProvider(initializer);
			isUpdateDisabled = initializer.IsUpdatedDisabled;
			logger = initializer.RemoteSettingsLogger;
		}

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
		public T GetValue<T>(string collectionPath, string key, T defaultValue)
		{
			if (TryGetValue(collectionPath, key, out T value))
			{
				return value;
			}
			return defaultValue;
		}

		/// <summary>
		/// Gets remote setting value if one exists.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the Remote Setting</param>
		/// <param name="value">The value or default(T)</param>
		/// <returns>True if value exists, false if it does not</returns>
		public bool TryGetValue<T>(string collectionPath, string key, out T value)
		{
			RequiresNotDisposed();
			CodeContract.RequiresArgumentNotNull<string>(collectionPath, "collectionPath");
			CodeContract.RequiresArgumentNotNull<string>(key, "key");
			bool flag = stableRemoteSettingProvider.IsStable(collectionPath);
			if (flag && stableRemoteSettingProvider.TryGetValue(collectionPath, key, out value))
			{
				return true;
			}
			foreach (IRemoteSettingsProvider remoteSettingsProvider in remoteSettingsProviders)
			{
				if (remoteSettingsProvider.TryGetValue(collectionPath, key, out value))
				{
					if (flag)
					{
						stableRemoteSettingProvider.MakeStable(collectionPath, key, value);
					}
					return true;
				}
			}
			value = default(T);
			return false;
		}

		/// <summary>
		/// Gets kind of a remote setting value.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the remote setting</param>
		/// <returns>Kind of the value or unknown if it does not exist or error.</returns>
		public ValueKind GetValueKind(string collectionPath, string key)
		{
			RequiresNotDisposed();
			CodeContract.RequiresArgumentNotNull<string>(collectionPath, "collectionPath");
			CodeContract.RequiresArgumentNotNull<string>(key, "key");
			ValueKind valueKind = ValueKind.Unknown;
			if (AllRemoteSettingsProviders.Any())
			{
				bool isFirst = true;
				ValueKind firstKind = valueKind;
				ValueKind currentKind;
				if (AllRemoteSettingsProviders.All(delegate(IRemoteSettingsProvider x)
				{
					if (x.TryGetValueKind(collectionPath, key, out currentKind))
					{
						if (isFirst)
						{
							firstKind = currentKind;
							isFirst = false;
						}
						return firstKind == currentKind;
					}
					return true;
				}))
				{
					valueKind = firstKind;
				}
			}
			return valueKind;
		}

		/// <summary>
		/// Gets a remote setting value, that is updated with both Targeted Notifications backend and RemoteControl
		/// file. Must be called after Start.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the remote setting</param>
		/// <param name="defaultValue">Value to return if remote setting does not exist</param>
		/// <returns>Remote setting value if it exists, otherwise defaultValue</returns>
		public async Task<T> GetValueAsync<T>(string collectionPath, string key, T defaultValue)
		{
			RequiresNotDisposed();
			RequiresStarted();
			CodeContract.RequiresArgumentNotNull<string>(collectionPath, collectionPath);
			CodeContract.RequiresArgumentNotNull<string>(key, key);
			foreach (IRemoteSettingsProvider allRemoteSettingsProvider in AllRemoteSettingsProviders)
			{
				RemoteSettingsProviderResult<T> remoteSettingsProviderResult = await allRemoteSettingsProvider.TryGetValueAsync<T>(collectionPath, key).ConfigureAwait(false);
				if (remoteSettingsProviderResult.RetrievalSuccessful)
				{
					return remoteSettingsProviderResult.Value;
				}
			}
			return defaultValue;
		}

		/// <summary>
		/// Gets all remote actions of type T, wrapped in ActionWrapper. Waits for the call to Targeted Notifications backend
		/// to complete. Must be called after Start.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="actionPath">Unique path to identify the actions to retrieve</param>
		/// <returns></returns>
		public async Task<IEnumerable<ActionWrapper<T>>> GetActionsAsync<T>(string actionPath)
		{
			RequiresNotDisposed();
			RequiresStarted();
			CodeContract.RequiresArgumentNotNull<string>(actionPath, "actionPath");
			IEnumerable<ActionWrapper<T>> enumerable = Enumerable.Empty<ActionWrapper<T>>();
			foreach (IRemoteSettingsProvider allRemoteSettingsProvider in AllRemoteSettingsProviders)
			{
				IEnumerable<ActionWrapper<T>> first = enumerable;
				enumerable = first.Concat(await allRemoteSettingsProvider.GetActionsAsync<T>(actionPath).ConfigureAwait(false));
			}
			return enumerable;
		}

		/// <summary>
		/// Subscribes to triggered remote actions of type T on the given action path.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="actionPath">Unique path to identify the actions to subscribe</param>
		/// <param name="callback">Callback to be invoked with each individual action when it becomes available</param>
		public void SubscribeActions<T>(string actionPath, Action<ActionWrapper<T>> callback)
		{
			RequiresNotDisposed();
			RequiresStarted();
			CodeContract.RequiresArgumentNotNull<string>(actionPath, "actionPath");
			CodeContract.RequiresArgumentNotNull<Action<ActionWrapper<T>>>(callback, "callback");
			foreach (IRemoteSettingsProvider allRemoteSettingsProvider in AllRemoteSettingsProviders)
			{
				allRemoteSettingsProvider.SubscribeActions(actionPath, callback);
			}
		}

		/// <summary>
		/// Unsubscribes to triggered remote actions on the given action path
		/// </summary>
		/// <param name="actionPath">Unique path to identify the actions to unsubscribe</param>
		public void UnsubscribeActions(string actionPath)
		{
			RequiresNotDisposed();
			RequiresStarted();
			CodeContract.RequiresArgumentNotNull<string>(actionPath, "actionPath");
			foreach (IRemoteSettingsProvider allRemoteSettingsProvider in AllRemoteSettingsProviders)
			{
				allRemoteSettingsProvider.UnsubscribeActions(actionPath);
			}
		}

		/// <summary>
		/// Starts a background operation to check for new Remote Settings and apply them.
		/// </summary>
		public void Start()
		{
			RequiresNotDisposed();
			if (!isStarted)
			{
				if (isUpdateDisabled())
				{
					StartTask = Task.FromResult(false);
				}
				else
				{
					List<Task<GroupedRemoteSettings>> tasks = new List<Task<GroupedRemoteSettings>>();
					foreach (IRemoteSettingsProvider allRemoteSettingsProvider in AllRemoteSettingsProviders)
					{
						tasks.Add(allRemoteSettingsProvider.Start());
					}
					StartTask = Task.Run(async delegate
					{
						await logger.Start().ConfigureAwait(false);
						IEnumerable<GroupedRemoteSettings> source = (await Task.WhenAll(tasks).ConfigureAwait(false)).Where((GroupedRemoteSettings x) => x != null);
						if ((nonScopedStorageHandler != null || logger.LoggingEnabled) && source.Any())
						{
							GroupedRemoteSettings groupedRemoteSettings = source.Reverse().Aggregate(delegate(GroupedRemoteSettings a, GroupedRemoteSettings b)
							{
								a.Merge(b, logger);
								return a;
							});
							logger.LogVerbose("Merged settings", groupedRemoteSettings);
							if (nonScopedStorageHandler != null)
							{
								using (Mutex mutex = new Mutex(false, "Global\\A7B8B64E-AEB3-4053-BC8C-C187F5320352"))
								{
									try
									{
										mutex.WaitOne(-1, false);
									}
									catch (AbandonedMutexException)
									{
									}
									try
									{
										nonScopedStorageHandler.DeleteAllSettings();
										nonScopedStorageHandler.SaveNonScopedSettings(groupedRemoteSettings);
									}
									catch
									{
									}
									finally
									{
										mutex.ReleaseMutex();
									}
								}
							}
						}
						OnRemoteSettingsApplied();
					});
				}
				isStarted = true;
			}
		}

		/// <summary>
		/// Add a scope filter provider.
		/// </summary>
		/// <param name="scopeFilterProvider">A filter provider</param>
		/// <returns>IRemoteSettings interface for chaining</returns>
		public IRemoteSettings RegisterFilterProvider(IScopeFilterProvider scopeFilterProvider)
		{
			CodeContract.RequiresArgumentNotNull<IScopeFilterProvider>(scopeFilterProvider, "scopeFilterProvider");
			scopeParserFactory.ProvidedFilters[scopeFilterProvider.Name] = scopeFilterProvider;
			return this;
		}

		/// <summary>
		/// Gets all the property names under a specific collection.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <returns>IEnumerable of all properties under the specified collection. Empty if no properties exist.</returns>
		public IEnumerable<string> GetPropertyNames(string collectionPath)
		{
			CodeContract.RequiresArgumentNotNull<string>(collectionPath, "collectionPath");
			return AllRemoteSettingsProviders.SelectMany((IRemoteSettingsProvider x) => x.GetPropertyNames(collectionPath)).Distinct();
		}

		/// <summary>
		/// Gets all the sub-collection names under a specific collection.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <returns>IEnumerable of the names of the sub collections. Empty if it does not exist.</returns>
		public IEnumerable<string> GetSubCollectionNames(string collectionPath)
		{
			CodeContract.RequiresArgumentNotNull<string>(collectionPath, "collectionPath");
			return AllRemoteSettingsProviders.SelectMany((IRemoteSettingsProvider x) => x.GetSubCollectionNames(collectionPath)).Distinct();
		}

		/// <summary>
		/// Determines if the collection exists.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <returns>True if the colleciton exists, otherwise false</returns>
		public bool CollectionExists(string collectionPath)
		{
			CodeContract.RequiresArgumentNotNull<string>(collectionPath, "collectionPath");
			return AllRemoteSettingsProviders.Any((IRemoteSettingsProvider x) => x.CollectionExists(collectionPath));
		}

		/// <summary>
		/// Determines if the property exists.
		/// </summary>
		/// <param name="collectionPath">Path to the remote setting collection in the form My\Custom\Path</param>
		/// <param name="key">Key of the Remote Setting</param>
		/// <returns>True if the property exists, otherwise false</returns>
		public bool PropertyExists(string collectionPath, string key)
		{
			CodeContract.RequiresArgumentNotNull<string>(collectionPath, collectionPath);
			CodeContract.RequiresArgumentNotNull<string>(key, key);
			return AllRemoteSettingsProviders.Any((IRemoteSettingsProvider x) => x.PropertyExists(collectionPath, key));
		}

		/// <inheritdoc />
		protected override void DisposeManagedResources()
		{
			foreach (IRemoteSettingsProvider allRemoteSettingsProvider in AllRemoteSettingsProviders)
			{
				allRemoteSettingsProvider.Dispose();
			}
			logger.Dispose();
		}

		private void RequiresStarted()
		{
			if (!isStarted)
			{
				throw new InvalidOperationException("Cannot access async methods until Start is called");
			}
		}

		private void OnRemoteSettingsApplied()
		{
			this.SettingsUpdated?.Invoke(this, EventArgs.Empty);
		}
	}
}
