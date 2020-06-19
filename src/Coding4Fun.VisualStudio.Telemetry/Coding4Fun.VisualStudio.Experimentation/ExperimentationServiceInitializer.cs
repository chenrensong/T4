using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Initializer class, which contains all settings necessary for ExperimentationService functionality.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public sealed class ExperimentationServiceInitializer
	{
		private static readonly string localTestFlightsPathSuffix = "LocalTest\\";

		private static readonly string enabledFlightsKey = "EnabledFlights";

		private static readonly string disabledFlightsKey = "DisabledFlights";

		private static readonly string shippedFlightsKey = "ShippedFlights";

		private static readonly string setFlightsKey = "SetFlights";

		/// <summary>
		/// Gets or sets telemetry service, which is used to send and set flights telemetry.
		/// </summary>
		public IExperimentationTelemetry ExperimentationTelemetry
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets filter provider, which provides filters necessary to build correct query to remote service.
		/// </summary>
		public IExperimentationFilterProvider ExperimentationFilterProvider
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets key-value storage necessary to keep local data, like cached flights.
		/// </summary>
		public IKeyValueStorage KeyValueStorage
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets experimentation optin status reader to understand whether we can start/work with remote services.
		/// </summary>
		public IExperimentationOptinStatusReader ExperimentationOptinStatusReader
		{
			get;
			set;
		}

		internal IFlightsProvider FlightsProvider
		{
			get;
			set;
		}

		internal SetFlightsProvider SetFlightsProvider
		{
			get;
			set;
		}

		internal IRemoteFileReaderFactory ShippedRemoteFileReaderFactory
		{
			get;
			set;
		}

		internal IRemoteFileReaderFactory DisabledSetRemoteFileReaderFactory
		{
			get;
			set;
		}

		internal IFlightsStreamParser FlightsStreamParser
		{
			get;
			set;
		}

		internal IHttpWebRequestFactory HttpWebRequestFactory
		{
			get;
			set;
		}

		internal IRegistryTools3 RegistryTools
		{
			get;
			set;
		}

		/// <summary>
		/// Create default instance of the initializer with default settings.
		/// </summary>
		/// <returns></returns>
		public static ExperimentationServiceInitializer BuildDefault()
		{
			return new ExperimentationServiceInitializer().FillWithDefaults();
		}

		/// <summary>
		/// Fill empty properties with default values without overriding initialized fields.
		/// </summary>
		/// <returns>Instance of the experimentation initializer</returns>
		public ExperimentationServiceInitializer FillWithDefaults()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			if (RegistryTools == null)
			{
				RegistryTools = (IRegistryTools3)(object)new RegistryTools();
			}
			if (ExperimentationFilterProvider == null)
			{
				ExperimentationFilterProvider = new DefaultExperimentationFilterProvider(TelemetryService.DefaultSession);
			}
			if (ExperimentationTelemetry == null)
			{
				ExperimentationTelemetry = new DefaultExperimentationTelemetry(TelemetryService.DefaultSession);
			}
			if (KeyValueStorage == null)
			{
				KeyValueStorage = new DefaultRegistryKeyValueStorage(RegistryTools);
			}
			if (ShippedRemoteFileReaderFactory == null)
			{
				ShippedRemoteFileReaderFactory = new ShippedFlightsRemoteFileReaderFactory();
			}
			if (DisabledSetRemoteFileReaderFactory == null)
			{
				DisabledSetRemoteFileReaderFactory = new DisabledFlightsRemoteFileReaderFactory();
			}
			if (FlightsStreamParser == null)
			{
				FlightsStreamParser = new JsonFlightsStreamParser();
			}
			if (HttpWebRequestFactory == null)
			{
				HttpWebRequestFactory = new HttpWebRequestFactory();
			}
			if (ExperimentationOptinStatusReader == null)
			{
				ExperimentationOptinStatusReader = new DefaultExperimentationOptinStatusReader(TelemetryService.DefaultSession, (IRegistryTools)(object)RegistryTools);
			}
			if (SetFlightsProvider == null)
			{
				SetFlightsProvider = new SetFlightsProvider(KeyValueStorage, setFlightsKey);
			}
			if (FlightsProvider == null)
			{
				RemoteFlightsProvider<ShippedFlightsData> remoteFlightsProvider = new RemoteFlightsProvider<ShippedFlightsData>(KeyValueStorage, shippedFlightsKey, ShippedRemoteFileReaderFactory, FlightsStreamParser);
				FlightsProvider = new MasterFlightsProvider(new IFlightsProvider[4]
				{
					new LocalFlightsProvider(KeyValueStorage, localTestFlightsPathSuffix + enabledFlightsKey),
					remoteFlightsProvider,
					new AFDFlightsProvider(KeyValueStorage, enabledFlightsKey, FlightsStreamParser, ExperimentationFilterProvider, HttpWebRequestFactory),
					SetFlightsProvider
				}, new IFlightsProvider[2]
				{
					new LocalFlightsProvider(KeyValueStorage, localTestFlightsPathSuffix + disabledFlightsKey),
					new RemoteFlightsProvider<DisabledFlightsData>(KeyValueStorage, disabledFlightsKey, DisabledSetRemoteFileReaderFactory, FlightsStreamParser)
				}, remoteFlightsProvider, ExperimentationOptinStatusReader);
			}
			return this;
		}
	}
}
