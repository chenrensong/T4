using Coding4Fun.VisualStudio.Experimentation;
using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Telemetry.Notification;
using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Initializer for Remote Settings
	/// </summary>
	public sealed class RemoteSettingsInitializer
	{
		/// <summary>
		/// Gets or sets whether to use default prefix for collection names in Key-Value storage.
		/// Like "Software\Coding4Fun\VisualStudio\RemoteSetting"
		/// </summary>
		public bool? UsePathPrefix
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the collection of ScopeFilterProviders that are registered to provide scopes.
		/// </summary>
		public IEnumerable<IScopeFilterProvider> ScopeFilterProviders
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the remote settings file name.
		/// </summary>
		public string RemoteSettingsFileName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the CollectionKeyValueStorage that will be used for the Remote Settings.
		/// </summary>
		public ICollectionKeyValueStorage KeyValueStorage
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the IExperimentationService that will be used to provide Flight scopes.
		/// </summary>
		public IExperimentationService ExperimentationService
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the ITelemetryNotificationProvider that will be used to subscribe to telemetry events.
		/// </summary>
		public ITelemetryNotificationService TelemetryNotificationService
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the telemetry implementation that will be used to log telemetry about Remote Settings operations.
		/// </summary>
		public IRemoteSettingsTelemetry Telemetry
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the implementation utilized for core Targeted Notifications telemetry.
		/// </summary>
		internal ITargetedNotificationsTelemetry TargetedNotificationsTelemetry
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the storage mechanism utilitized for Targeted Notifications client side caching.
		/// </summary>
		internal ITargetedNotificationsCacheStorageProvider TargetedNotificationsCacheStorage
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the filter provider that will be used to provide values for the Targeted Notifications request plus Scope Providers.
		/// </summary>
		public RemoteSettingsFilterProvider FilterProvider
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the key value storage that will be used for non scoped settings, as a way for consumers to access RemoteSettings directly without
		/// having to go through this library.
		/// </summary>
		public ICollectionKeyValueStorage NonScopedSettingsKeyValueStorage
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the names of root subcollections that will hold stable settings.
		/// The first time a setting under one of these subcollections gets requested, the value is cached,
		/// and RemoteSettings will continue to return this value for the rest of the process lifetime.
		/// </summary>
		public IEnumerable<string> StableSettingRootSubCollections
		{
			get;
			set;
		}

		internal IScopeParserFactory ScopeParserFactory
		{
			get;
			set;
		}

		internal IVersionedRemoteSettingsStorageHandler VersionedRemoteSettingsStorageHandler
		{
			get;
			set;
		}

		internal IRemoteSettingsStorageHandler CacheableRemoteSettingsStorageHandler
		{
			get;
			set;
		}

		internal IRemoteSettingsStorageHandler LocalTestRemoteSettingsStorageHandler
		{
			get;
			set;
		}

		internal IEnumerable<IDirectoryReader> LocalTestDirectories
		{
			get;
			set;
		}

		internal Func<IRemoteSettingsStorageHandler> LiveRemoteSettingsStorageHandlerFactory
		{
			get;
			set;
		}

		internal IRemoteSettingsStorageHandler NonScopedRemoteSettingsStorageHandler
		{
			get;
			set;
		}

		internal IRemoteFileReaderFactory RemoteFileReaderFactory
		{
			get;
			set;
		}

		internal IRemoteSettingsParser RemoteSettingsParser
		{
			get;
			set;
		}

		internal ITargetedNotificationsParser TargetedNotificationsParser
		{
			get;
			set;
		}

		internal IRemoteSettingsValidator RemoteSettingsValidator
		{
			get;
			set;
		}

		internal IEnumerable<Func<RemoteSettingsInitializer, IRemoteSettingsProvider>> RemoteSettingsProviders
		{
			get;
			set;
		}

		internal Func<RemoteSettingsInitializer, IStableRemoteSettingsProvider> StableRemoteSettingsProvider
		{
			get;
			set;
		}

		internal IHttpWebRequestFactory HttpWebRequestFactory
		{
			get;
			set;
		}

		internal Func<bool> IsUpdatedDisabled
		{
			get;
			set;
		}

		internal ILocalTestParser LocalTestParser
		{
			get;
			set;
		}

		internal IRemoteSettingsLogger RemoteSettingsLogger
		{
			get;
			set;
		}

		internal static RemoteSettingsInitializer BuildDefault()
		{
			return new RemoteSettingsInitializer().FillWithDefaults();
		}

		internal RemoteSettingsInitializer FillWithDefaults()
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			if (!UsePathPrefix.HasValue)
			{
				UsePathPrefix = true;
			}
			if (RemoteSettingsFileName == null)
			{
				RemoteSettingsFileName = "Default.json";
			}
			if (KeyValueStorage == null)
			{
				KeyValueStorage = new DefaultRegistryKeyValueStorage((IRegistryTools3)(object)new RegistryTools());
			}
			if (FilterProvider == null)
			{
				FilterProvider = new DefaultRemoteSettingsFilterProvider(TelemetryService.DefaultSession);
			}
			if (RemoteSettingsLogger == null)
			{
				RemoteSettingsLogger = new RemoteSettingsLogger(FilterProvider, RemoteSettingsStorageHandlerFactory.BuildIsLoggingEnabled(KeyValueStorage, UsePathPrefix.HasValue && UsePathPrefix.Value)());
			}
			if (ScopeParserFactory == null)
			{
				ScopeParserFactory = new ScopeParserFactory(this);
			}
			RemoteSettingsStorageHandlerFactory remoteSettingsStorageHandlerFactory = new RemoteSettingsStorageHandlerFactory(RemoteSettingsLogger);
			if (VersionedRemoteSettingsStorageHandler == null)
			{
				VersionedRemoteSettingsStorageHandler = remoteSettingsStorageHandlerFactory.BuildVersioned(KeyValueStorage, UsePathPrefix.HasValue && UsePathPrefix.Value, RemoteSettingsFileName, ScopeParserFactory);
			}
			if (CacheableRemoteSettingsStorageHandler == null)
			{
				CacheableRemoteSettingsStorageHandler = remoteSettingsStorageHandlerFactory.Build(KeyValueStorage, UsePathPrefix.HasValue && UsePathPrefix.Value, FilterProvider, ScopeParserFactory);
			}
			if (LocalTestRemoteSettingsStorageHandler == null)
			{
				LocalTestRemoteSettingsStorageHandler = remoteSettingsStorageHandlerFactory.Build(KeyValueStorage, UsePathPrefix.HasValue && UsePathPrefix.Value, "LocalTest", ScopeParserFactory);
			}
			if (LiveRemoteSettingsStorageHandlerFactory == null)
			{
				LiveRemoteSettingsStorageHandlerFactory = (() => remoteSettingsStorageHandlerFactory.Build(new MemoryKeyValueStorage(), false, string.Empty, ScopeParserFactory));
			}
			if (NonScopedRemoteSettingsStorageHandler == null && NonScopedSettingsKeyValueStorage != null)
			{
				NonScopedRemoteSettingsStorageHandler = remoteSettingsStorageHandlerFactory.Build(NonScopedSettingsKeyValueStorage, false, string.Empty, ScopeParserFactory);
			}
			if (IsUpdatedDisabled == null)
			{
				IsUpdatedDisabled = RemoteSettingsStorageHandlerFactory.BuildIsUpdateDisabled(KeyValueStorage, UsePathPrefix.HasValue && UsePathPrefix.Value);
			}
			if (RemoteFileReaderFactory == null)
			{
				RemoteFileReaderFactory = new RemoteSettingsRemoteFileReaderFactory(RemoteSettingsFileName);
			}
			if (RemoteSettingsValidator == null)
			{
				RemoteSettingsValidator = new RemoteSettingsValidator(new CycleDetection(), VersionedRemoteSettingsStorageHandler);
			}
			if (RemoteSettingsParser == null)
			{
				RemoteSettingsParser = new RemoteSettingsParser(RemoteSettingsValidator);
			}
			if (TargetedNotificationsParser == null)
			{
				TargetedNotificationsParser = new TargetedNotificationsParser();
			}
			if (ExperimentationService == null)
			{
				ExperimentationService = Coding4Fun.VisualStudio.Experimentation.ExperimentationService.Default;
			}
			if (ExperimentationService == null)
			{
				ExperimentationService = Coding4Fun.VisualStudio.Experimentation.ExperimentationService.Default;
			}
			if (TelemetryNotificationService == null)
			{
				TelemetryNotificationService = Coding4Fun.VisualStudio.Telemetry.Notification.TelemetryNotificationService.Default;
			}
			if (Telemetry == null)
			{
				Telemetry = new DefaultRemoteSettingsTelemetry(TelemetryService.DefaultSession);
			}
			if (TargetedNotificationsTelemetry == null)
			{
				TargetedNotificationsTelemetry = new DefaultTargetedNotificationsTelemetry(TelemetryService.DefaultSession);
			}
			if (TargetedNotificationsCacheStorage == null)
			{
				TargetedNotificationsCacheStorage = new TargetedNotificationsJsonStorageProvider(this);
			}
			if (HttpWebRequestFactory == null)
			{
				HttpWebRequestFactory = new HttpWebRequestFactory();
			}
			if (ScopeFilterProviders == null)
			{
				new ProcessInformationProvider();
				ScopeFilterProviders = new List<IScopeFilterProvider>
				{
					new FlightScopeFilterProvider(ExperimentationService),
					new InternalScopeFilterProvider(TelemetryService.DefaultSession),
					new VersionScopeFilterProvider(FilterProvider),
					new ExeNameScopeFilterProvider(FilterProvider),
					new ScopeScopeFilterProvider(VersionedRemoteSettingsStorageHandler, ScopeParserFactory)
				};
			}
			if (RemoteSettingsProviders == null)
			{
				RemoteSettingsProviders = new List<Func<RemoteSettingsInitializer, IRemoteSettingsProvider>>
				{
					(RemoteSettingsInitializer remoteSettingsInitializer) => new LocalTestProvider(remoteSettingsInitializer),
					(RemoteSettingsInitializer remoteSettingsInitializer) => new TargetedNotificationsProvider(remoteSettingsInitializer),
					(RemoteSettingsInitializer remoteSettingsInitializer) => new RemoteControlRemoteSettingsProvider(remoteSettingsInitializer)
				};
			}
			if (LocalTestDirectories == null)
			{
				string rootPath = Path.Combine(GetLocalAppDataRoot(), "LocalTest");
				LocalTestDirectories = new List<IDirectoryReader>
				{
					new DirectoryReader(rootPath, "PersistentActions", false, 0, RemoteSettingsLogger),
					new DirectoryReader(rootPath, "OneTimeActions", true, 10, RemoteSettingsLogger)
				};
			}
			if (LocalTestParser == null)
			{
				LocalTestParser = new LocalTestParser();
			}
			if (StableRemoteSettingsProvider == null)
			{
				StableRemoteSettingsProvider = ((RemoteSettingsInitializer remoteSettingsInitializer) => new StableRemoteSettingsProvider(remoteSettingsInitializer));
			}
			if (StableSettingRootSubCollections == null)
			{
				StableSettingRootSubCollections = Enumerable.Empty<string>();
			}
			return this;
		}

		internal string GetLocalAppDataRoot()
		{
			try
			{
				string path;
				if (Platform.IsMac)
				{
					path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Application Support");
				}
				else if (Platform.IsLinux)
				{
					string text = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
					if (string.IsNullOrEmpty(text))
					{
						text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".cache");
					}
					path = text;
				}
				else
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				}
				return Path.Combine(path, "Coding4Fun", "VisualStudio", "RemoteSettings");
			}
			catch
			{
			}
			return string.Empty;
		}
	}
}
