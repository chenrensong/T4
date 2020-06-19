using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Initial telemetry session configuration
	/// </summary>
	internal sealed class TelemetrySessionInitializer
	{
		private const string DefaultAsimovIKey = "AIF-312cbd79-9dbb-4c48-a7da-3cc2a931cb70";

		private const string DefaultAppInsightsIKey = "f144292e-e3b2-4011-ac90-20e5c03fbce5";

		public CancellationTokenSource CancellationTokenSource
		{
			get;
			set;
		}

		public string SessionId
		{
			get;
			set;
		}

		public string AppInsightsInstrumentationKey
		{
			get;
			set;
		}

		public string AsimovInstrumentationKey
		{
			get;
			set;
		}

		public IDiagnosticTelemetry DiagnosticTelemetry
		{
			get;
			set;
		}

		public IIdentityTelemetry IdentityTelemetry
		{
			get;
			set;
		}

		public IEnvironmentTools EnvironmentTools
		{
			get;
			set;
		}

		public IRegistryTools2 RegistryTools
		{
			get;
			set;
		}

		public ITelemetryOptinStatusReader OptinStatusReader
		{
			get;
			set;
		}

		public IProcessTools ProcessTools
		{
			get;
			set;
		}

		public ILegacyApi LegacyApi
		{
			get;
			set;
		}

		public IInternalSettings InternalSettings
		{
			get;
			set;
		}

		public IHostInformationProvider HostInformationProvider
		{
			get;
			set;
		}

		public IMachineInformationProvider MachineInformationProvider
		{
			get;
			set;
		}

		public IMACInformationProvider MACInformationProvider
		{
			get;
			set;
		}

		public IUserInformationProvider UserInformationProvider
		{
			get;
			set;
		}

		public ITelemetryScheduler EventProcessorScheduler
		{
			get;
			set;
		}

		public ITelemetryScheduler ContextScheduler
		{
			get;
			set;
		}

		public ITelemetryManifestManagerBuilder TelemetryManifestManagerBuilder
		{
			get;
			set;
		}

		public IContextPropertyManager DefaultContextPropertyManager
		{
			get;
			set;
		}

		public IPersistentPropertyBag PersistentStorage
		{
			get;
			set;
		}

		public IPersistentPropertyBag PersistentSharedProperties
		{
			get;
			set;
		}

		public IPersistentPropertyBag PersistentPropertyBag
		{
			get;
			set;
		}

		public IEnumerable<IChannelValidator> ChannelValidators
		{
			get;
			set;
		}

		public IEnumerable<ISessionChannel> ChannelsToAdd
		{
			get;
			set;
		}

		public IEnumerable<IEventProcessorAction> CustomActionToAdd
		{
			get;
			set;
		}

		public IEnumerable<IPropertyProvider> PropertyProviders
		{
			get;
			set;
		}

		public EventProcessorChannelBuilder EventProcessorChannelBuilder
		{
			get;
			set;
		}

		public WatsonSessionChannelBuilder WatsonSessionChannelBuilder
		{
			get;
			set;
		}

		public IProcessCreationTime ProcessCreationTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets default session initializer
		/// </summary>
		public static TelemetrySessionInitializer Default => BuildInitializer(null);

		/// <summary>
		/// Build default session initializer, but modify components according to session settings.
		/// </summary>
		/// <param name="telemetrySessionSettings"></param>
		/// <returns></returns>
		public static TelemetrySessionInitializer FromSessionSettings(TelemetrySessionSettings telemetrySessionSettings)
		{
			return BuildInitializer(telemetrySessionSettings);
		}

		/// <summary>
		/// Create default session channels and return IEnumerable list of them.
		/// In the case when checkPendingAsimovEvents == true call method on Vortex channel
		/// to check whether pending telemetry is exists and in case existing Start this channel
		/// immediately.
		/// </summary>
		/// <param name="telemetrySession"></param>
		/// <param name="checkPendingAsimovEvents"></param>
		/// <returns></returns>
		public IEnumerable<ISessionChannel> CreateSessionChannels(TelemetrySession telemetrySession, bool checkPendingAsimovEvents)
		{
			IStorageBuilder storageBuilder = GetStorageBuilder();
			IProcessLockFactory processLockFactory = GetProcessLock();
			if (AppInsightsInstrumentationKey != null)
			{
				yield return new DefaultAppInsightsSessionChannel(AppInsightsInstrumentationKey, telemetrySession.UserId.ToString(), storageBuilder, processLockFactory);
			}
			if (AsimovInstrumentationKey != null)
			{
				AsimovAppInsightsSessionChannel asimovAppInsightsSessionChannel = new AsimovAppInsightsSessionChannel("aivortex", false, AsimovInstrumentationKey, telemetrySession.UserId.ToString(), ChannelProperties.Default | ChannelProperties.NotForUnitTest, telemetrySession, storageBuilder, processLockFactory);
				if (checkPendingAsimovEvents)
				{
					asimovAppInsightsSessionChannel.CheckPendingEventsAndStartChannel(telemetrySession.SessionId);
				}
				yield return asimovAppInsightsSessionChannel;
				yield return new AsimovAppInsightsSessionChannel("aiasimov", Platform.IsWindows, AsimovInstrumentationKey, telemetrySession.UserId.ToString(), ChannelProperties.NotForUnitTest, telemetrySession, storageBuilder, processLockFactory);
			}
			yield return new TelemetryLogToFileChannel();
		}

		/// <summary>
		/// Validate initializer for the TelemetrySession
		/// </summary>
		public void Validate()
		{
			CodeContract.RequiresArgumentNotEmptyOrWhitespace(AppInsightsInstrumentationKey, "AppInsightsInstrumentationKey");
			CodeContract.RequiresArgumentNotEmptyOrWhitespace(AsimovInstrumentationKey, "AsimovInstrumentationKey");
			CodeContract.RequiresArgumentNotNull<IDiagnosticTelemetry>(DiagnosticTelemetry, "DiagnosticTelemetry");
			CodeContract.RequiresArgumentNotNull<IHostInformationProvider>(HostInformationProvider, "HostInformationProvider");
			CodeContract.RequiresArgumentNotNull<IMachineInformationProvider>(MachineInformationProvider, "MachineInformationProvider");
			CodeContract.RequiresArgumentNotNull<IUserInformationProvider>(UserInformationProvider, "UserInformationProvider");
			CodeContract.RequiresArgumentNotNull<ITelemetryScheduler>(EventProcessorScheduler, "EventProcessorScheduler");
			CodeContract.RequiresArgumentNotNull<IContextPropertyManager>(DefaultContextPropertyManager, "DefaultContextPropertyManager");
			CodeContract.RequiresArgumentNotNull<IEnumerable<IChannelValidator>>(ChannelValidators, "ChannelValidators");
			CodeContract.RequiresArgumentNotNull<ITelemetryOptinStatusReader>(OptinStatusReader, "OptinStatusReader");
		}

		private static TelemetrySessionInitializer BuildInitializer(TelemetrySessionSettings telemetrySessionSettings)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			DiagnosticTelemetry diagnosticTelemetry = new DiagnosticTelemetry();
			EnvironmentTools environmentTools = new EnvironmentTools();
			RegistryTools registryTools = (RegistryTools)(object)new RegistryTools();
			ProcessTools processTools = new ProcessTools();
			ILegacyApi legacyApi = GetLegacyApi((IRegistryTools)(object)registryTools);
			IPersistentPropertyBag persistentStorage = CreatePersistentPropertyBag(string.Empty);
			IPersistentPropertyBag persistentPropertyBag = CreatePersistentPropertyBag("c57a9efce9b74de382d905a89852db71");
			IMACInformationProvider mACInformationProvider = GetMACInformationProvider(processTools, persistentStorage, legacyApi);
			IInternalSettings internalSettings = GetInternalSettings(diagnosticTelemetry, (IRegistryTools)(object)registryTools);
			IHostInformationProvider hostInformationProvider = GetHostInformationProvider();
			IUserInformationProvider userInformationProvider = GetUserInformationProvider((IRegistryTools)(object)registryTools, internalSettings, environmentTools, legacyApi, telemetrySessionSettings);
			MachineInformationProvider machineInformationProvider = new MachineInformationProvider(legacyApi, userInformationProvider, mACInformationProvider);
			IIdentityInformationProvider identityInformationProvider = GetIdentityInformationProvider();
			IPersistentPropertyBag persistentPropertyBag2 = CreatePersistentPropertyBag(hostInformationProvider.ProcessName);
			TelemetryScheduler telemetryScheduler = new TelemetryScheduler();
			OptOutAction optOutAction = new OptOutAction();
			optOutAction.AddOptOutFriendlyEventName("vs/telemetryapi/session/initialized");
			optOutAction.AddOptOutFriendlyEventName("context/postproperty");
			optOutAction.AddOptOutFriendlyPropertiesList(new List<string>
			{
				"Context.Default.VS.Core.BranchName",
				"Context.Default.VS.Core.BuildNumber",
				"Context.Default.VS.Core.ExeName",
				"Context.Default.VS.Core.ExeVersion",
				"Context.Default.VS.Core.HardwareId",
				"Context.Default.VS.Core.MacAddressHash",
				"Context.Default.VS.Core.Machine.Id",
				"Context.Default.VS.Core.SkuName",
				"Context.Default.VS.Core.OS.Version",
				"Context.Default.VS.Core.ProcessId",
				"Context.Default.VS.Core.User.Id",
				"Context.Default.VS.Core.User.IsMicrosoftInternal",
				"Context.Default.VS.Core.User.IsOptedIn",
				"Context.Default.VS.Core.User.Location.GeoId",
				"Context.Default.VS.Core.User.Type",
				"Context.Default.VS.Core.Version",
				"Reserved.EventId",
				"Reserved.SessionId",
				"Reserved.TimeSinceSessionStart",
				"VS.Core.Locale.Product",
				"VS.Core.Locale.System",
				"VS.Core.Locale.User",
				"VS.Core.Locale.UserUI",
				"VS.Core.SkuId",
				"VS.Sqm.SkuId"
			});
			ClientSideThrottlingAction item = new ClientSideThrottlingAction(new List<string>
			{
				"context/create",
				"context/close",
				"context/postproperty",
				"vs/core/command",
				"vs/core/extension/installed",
				"vs/core/sessionstart",
				"vs/core/sessionend",
				"vs/telemetryapi/session/initialized",
				"vs/telemetryapi/clientsidethrottling",
				"vs/telemetryapi/manifest/load",
				"vs/telemetryapi/piiproperties"
			}, 0.0, 0L);
			PIIPropertyProcessor pIIPropertyProcessor = new PIIPropertyProcessor();
			PiiAction item2 = new PiiAction(pIIPropertyProcessor);
			ComplexPropertyAction item3 = new ComplexPropertyAction(new JsonComplexObjectSerializerFactory(), pIIPropertyProcessor);
			return new TelemetrySessionInitializer
			{
				CancellationTokenSource = cancellationTokenSource,
				SessionId = Guid.NewGuid().ToString(),
				AppInsightsInstrumentationKey = "f144292e-e3b2-4011-ac90-20e5c03fbce5",
				AsimovInstrumentationKey = "AIF-312cbd79-9dbb-4c48-a7da-3cc2a931cb70",
				DiagnosticTelemetry = diagnosticTelemetry,
				IdentityTelemetry = new IdentityTelemetry(identityInformationProvider, telemetryScheduler),
				EnvironmentTools = environmentTools,
				RegistryTools = (IRegistryTools2)(object)registryTools,
				ProcessTools = processTools,
				LegacyApi = legacyApi,
				InternalSettings = internalSettings,
				HostInformationProvider = hostInformationProvider,
				MachineInformationProvider = machineInformationProvider,
				UserInformationProvider = userInformationProvider,
				MACInformationProvider = mACInformationProvider,
				EventProcessorScheduler = telemetryScheduler,
				TelemetryManifestManagerBuilder = new TelemetryManifestManagerBuilder(),
				DefaultContextPropertyManager = new DefaultContextPropertyManager(GetPropertyProviders((IRegistryTools)(object)registryTools, environmentTools, hostInformationProvider, machineInformationProvider, mACInformationProvider, userInformationProvider, persistentPropertyBag, identityInformationProvider)),
				PersistentStorage = persistentStorage,
				PersistentSharedProperties = persistentPropertyBag,
				PersistentPropertyBag = persistentPropertyBag2,
				ChannelValidators = new List<IChannelValidator>
				{
					new RegistryChannelValidator(internalSettings),
					new InternalChannelValidator(userInformationProvider),
					new DisabledTelemetryChannelValidator(internalSettings)
				},
				CustomActionToAdd = new List<IEventProcessorAction>
				{
					optOutAction,
					new EnforceAIRestrictionAction(),
					item2,
					item3,
					new MetricAction(),
					new SettingAction(),
					item,
					new SuppressEmptyPostPropertyEventAction()
				},
				EventProcessorChannelBuilder = new EventProcessorChannelBuilder(persistentPropertyBag2, telemetryScheduler),
				WatsonSessionChannelBuilder = new WatsonSessionChannelBuilder(internalSettings.FaultEventWatsonSamplePercent(), internalSettings.FaultEventMaximumWatsonReportsPerSession(), internalSettings.FaultEventMinimumSecondsBetweenWatsonReports(), ChannelProperties.Default),
				OptinStatusReader = GetOptInStatusReader((IRegistryTools2)(object)registryTools),
				ProcessCreationTime = GetProcessCreationTime()
			};
		}

		private static ILegacyApi GetLegacyApi(IRegistryTools registryTools)
		{
			if (Platform.IsWindows)
			{
				return new LegacyApi(registryTools);
			}
			return new MonoLegacyApi(registryTools);
		}

		private static IMACInformationProvider GetMACInformationProvider(IProcessTools processTools, IPersistentPropertyBag persistentStorage, ILegacyApi legacyApi)
		{
			if (Platform.IsWindows)
			{
				return new WindowsMACInformationProvider(processTools, persistentStorage);
			}
			if (Platform.IsMac)
			{
				return new MacMACInformationProvider(persistentStorage, legacyApi);
			}
			return new MonoMACInformationProvider(processTools, persistentStorage);
		}

		private static IHostInformationProvider GetHostInformationProvider()
		{
			if (Platform.IsWindows)
			{
				return new WindowsHostInformationProvider();
			}
			return new MonoHostInformationProvider();
		}

		private static IIdentityInformationProvider GetIdentityInformationProvider()
		{
			if (Platform.IsWindows)
			{
				return new WindowsIdentityInformationProvider();
			}
			return new MonoIdentityInformationProvider();
		}

		private static IUserInformationProvider GetUserInformationProvider(IRegistryTools registryTools, IInternalSettings internalSettings, IEnvironmentTools environmentTools, ILegacyApi legacyApi, TelemetrySessionSettings telemetrySessionSettings)
		{
			if (Platform.IsWindows)
			{
				return new WindowsUserInformationProvider(registryTools, internalSettings, environmentTools, legacyApi, telemetrySessionSettings?.UserId);
			}
			return new MonoUserInformationProvider(internalSettings, environmentTools, legacyApi, telemetrySessionSettings?.UserId);
		}

		private static IEnumerable<IPropertyProvider> GetPropertyProviders(IRegistryTools registryTools, IEnvironmentTools environmentTools, IHostInformationProvider host, IMachineInformationProvider machine, IMACInformationProvider macAddress, IUserInformationProvider user, IPersistentPropertyBag sharedProperties, IIdentityInformationProvider identity)
		{
			yield return new IdentityPropertyProvider();
			if (Platform.IsMac)
			{
				yield return new MacHostPropertyProvider(host, new NsBundleInformationProvider());
				yield return new AssemblyPropertyProvider();
				yield return new MacLocalePropertyProvider();
				yield return new MacMachinePropertyProvider(machine, registryTools, macAddress);
				yield return new MacOSPropertyProvider(environmentTools);
				yield return new MacUserPropertyProvider(user);
				yield return new PersistentSharedPropertyProvider(sharedProperties);
			}
			else if (Platform.IsLinux)
			{
				yield return new LinuxHostPropertyProvider(host);
				yield return new AssemblyPropertyProvider();
				yield return new LinuxLocalePropertyProvider();
				yield return new PersistentSharedPropertyProvider(sharedProperties);
			}
			else
			{
				yield return new WindowsHostPropertyProvider(host);
				yield return new AssemblyPropertyProvider();
				yield return new WindowsLocalePropertyProvider(registryTools);
				yield return new WindowsMachinePropertyProvider(machine, registryTools, macAddress);
				yield return new WindowsOSPropertyProvider(environmentTools, registryTools);
				yield return new WindowsUserPropertyProvider(user);
				yield return new PersistentSharedPropertyProvider(sharedProperties);
			}
		}

		private static IStorageBuilder GetStorageBuilder()
		{
			if (Platform.IsMac)
			{
				return new MacStorageBuilder();
			}
			if (Platform.IsLinux)
			{
				return new LinuxStorageBuilder();
			}
			return new WindowsStorageBuilder();
		}

		internal static IProcessCreationTime GetProcessCreationTime()
		{
			if (Platform.IsWindows)
			{
				return new WindowsProcessCreationTime();
			}
			return new MonoProcessCreationTime();
		}

		private static IInternalSettings GetInternalSettings(IDiagnosticTelemetry diagnosticTelemetry, IRegistryTools registryTools)
		{
			if (Platform.IsWindows)
			{
				return new WindowsInternalSettings(diagnosticTelemetry, registryTools);
			}
			return new MonoInternalSettings(diagnosticTelemetry, registryTools);
		}

		private static ITelemetryOptinStatusReader GetOptInStatusReader(IRegistryTools2 registryTools)
		{
			if (Platform.IsWindows)
			{
				return new TelemetryVsOptinStatusReader(registryTools);
			}
			if (Platform.IsMac)
			{
				return new MacVsOptinStatusReader();
			}
			return new NullVsOptinStatusReader();
		}

		private static IPersistentPropertyBag CreatePersistentPropertyBag(string processName)
		{
			if (Platform.IsWindows)
			{
				return new RegistryPropertyBag(processName);
			}
			return new MonoRegistryPropertyBag(processName);
		}

		private static IProcessLockFactory GetProcessLock()
		{
			if (Platform.IsWindows)
			{
				return new WindowsProcessLockFactory();
			}
			return new MonoProcessLockFactory();
		}
	}
}
