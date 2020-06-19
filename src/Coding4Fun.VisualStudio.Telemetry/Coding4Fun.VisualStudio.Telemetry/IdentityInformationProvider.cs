using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	///     OS independent Identity information provier logic. Use the Provider specific to the target OS.
	/// </summary>
	internal abstract class IdentityInformationProvider : IIdentityInformationProvider
	{
		internal class MachineIdentitifier
		{
			public enum IdType
			{
				PrimaryImmutableId,
				PrimaryCombinedId,
				PrimaryId,
				Secondary,
				Informational
			}

			protected readonly Lazy<IPersistentPropertyBag> lazyIdStore;

			protected Func<string> getCurrentStoredValue;

			protected Lazy<string> lazyCurrentSystemValueUnhashed;

			protected Lazy<bool> lazyIsValid;

			private Lazy<string> lazyGetCurrentSystemValue;

			public string Name
			{
				get;
			}

			public IdType Type
			{
				get;
			}

			public string CurrentSystemValueUnhased => lazyCurrentSystemValueUnhashed.Value;

			/// <summary>
			///     Gets the id value that is currently in the in memory copy of the store.
			/// </summary>
			public string CurrentStoredValue => getCurrentStoredValue();

			/// <summary>
			///     Gets the valid value between what was last persisted and the current value collected from the system.
			/// </summary>
			public string ValidValue
			{
				get
				{
					if (!IsLastPersistedValueValid)
					{
						return CurrentSystemValue;
					}
					return LastPersistedValue;
				}
			}

			/// <summary>
			///     Gets or setsthe original value that was read from the physical store.
			/// </summary>
			public string LastPersistedValue
			{
				get;
				protected set;
			}

			/// <summary>
			///     Gets a value indicating whether the last persisted value is valid.
			/// </summary>
			public bool IsLastPersistedValueValid => lazyIsValid.Value;

			private IPersistentPropertyBag IdStore => lazyIdStore.Value;

			public string CurrentSystemValue => lazyGetCurrentSystemValue.Value;

			internal MachineIdentitifier(Lazy<IPersistentPropertyBag> store, string name, IdType type, Func<string> getCurrentSystemValue, Func<string, bool> validateValue = null, string[] comboIds = null, Func<string, bool> primaryIdStillExists = null)
			{
				lazyIdStore = store;
				Name = name;
				Type = type;
				lazyCurrentSystemValueUnhashed = new Lazy<string>(getCurrentSystemValue);
				lazyGetCurrentSystemValue = new Lazy<string>(delegate
				{
					lock (LazyPiiPropertyProcessorLockObject)
					{
						return (type == IdType.Informational) ? CurrentSystemValueUnhased : LazyPiiPropertyProcessor.Value.ConvertToHashedValue(new TelemetryPiiProperty(CurrentSystemValueUnhased));
					}
				});
				if (type != IdType.PrimaryCombinedId)
				{
					LastPersistedValue = GetStoreProperty(Name);
				}
				getCurrentStoredValue = (() => GetStoreProperty(Name));
				lazyIsValid = new Lazy<bool>(() => !string.IsNullOrWhiteSpace(LastPersistedValue) && ((validateValue == null) ? object.Equals(LastPersistedValue, CurrentSystemValue) : validateValue(LastPersistedValue)));
			}

			/// <summary>
			///     Stores the CurrentValid value if it is different than the previously stored value.
			/// </summary>
			public void StoreValidValue()
			{
				if (CurrentStoredValue != ValidValue)
				{
					SetStoreProperty(Name, ValidValue);
				}
			}

			private string GetStoreProperty(string propertyName)
			{
				return IdStore.GetProperty(StoredPropertyNamesToInts[propertyName]) as string;
			}

			private void SetStoreProperty(string propertyName, string val)
			{
				IdStore.SetProperty(StoredPropertyNamesToInts[propertyName], val);
			}
		}

		internal class MachinePrimaryIdentitifier : MachineIdentitifier
		{
			private readonly Lazy<bool> lazyPrimaryIdStillExists;

			internal bool PrimaryIdStillExists => lazyPrimaryIdStillExists.Value;

			public MachinePrimaryIdentitifier(Lazy<IPersistentPropertyBag> lazyIdStore, string name, Func<string> getCurrentValue, Func<string, bool> validateValue, Func<string, bool> primaryIdStillExists)
				: base(lazyIdStore, name, IdType.PrimaryId, getCurrentValue, validateValue)
			{
				lazyPrimaryIdStillExists = new Lazy<bool>(() => primaryIdStillExists(base.LastPersistedValue));
				lazyIsValid = new Lazy<bool>(() => !string.IsNullOrWhiteSpace(base.LastPersistedValue) && (validateValue(base.LastPersistedValue) || primaryIdStillExists(base.LastPersistedValue)));
			}
		}

		internal class MachineComboIdentitifier : MachineIdentitifier
		{
			public MachineComboIdentitifier(Lazy<IPersistentPropertyBag> lazyIdStore, string name, Func<string> getCurrentValue, Func<IEnumerable<MachineIdentitifier>> getComboIds)
				: base(lazyIdStore, name, IdType.PrimaryCombinedId, getCurrentValue)
			{
				base.LastPersistedValue = JoinIdentifiers(getComboIds(), (MachineIdentitifier id) => id.LastPersistedValue);
				getCurrentStoredValue = (() => null);
				lazyIsValid = new Lazy<bool>(delegate
				{
					lock (LazyPiiPropertyProcessorLockObject)
					{
						return !string.IsNullOrWhiteSpace(base.LastPersistedValue) && object.Equals(base.CurrentSystemValue, LazyPiiPropertyProcessor.Value.ConvertToHashedValue(new TelemetryPiiProperty(base.LastPersistedValue)));
					}
				});
			}
		}

		public static readonly string HardwareIdEventFaultName = "VS/TelemetryApi/Identity/HardwareIdFault";

		internal const string PersistedIdentityNotFoundTemplate = "Persisted identity '{0}' was not found.";

		internal const string IdChangeCausedInvalidationTemplate = "{0} change requires invalidation of persisted ID.";

		internal const string EventParameterLastValueSuffix = ".Last";

		internal const string ConfigVersionInvalidationTemplate = "Persisted config version '{0}' is less than MinValidConfigVersion '{1}'";

		internal const string PrimaryIdValueNoLongerExistsTemplate = "PrimaryId '{0}' value no longer found on machine.";

		internal const string PrimaryIdValueNoLongerExists = "PrimaryIdValueNoLongerExists";

		internal const string HardwarePersistenceDate = "HardwareIdDate";

		internal const string HardwarePersistenceAge = "HardwareIdAge";

		internal const string IsMachineStoreAccessiblePropertyName = "IsMachineStoreAccessible";

		internal static readonly string HardwareIdNotObtained = "HardwareId not obtained";

		internal static readonly string HardwareIdNotPeristed = "HardwareId not persisted.";

		private const string ExceptionObtainingHardwareIdFaultDescription = "ExceptionObtainingHardwareId";

		private static readonly Lazy<IPGlobalProperties> IpGlobalProperties = new Lazy<IPGlobalProperties>(InitializeIpGlobalProperties);

		private readonly Lazy<IPersistentPropertyBag> lazyMachineStore;

		private readonly Lazy<Dictionary<string, MachineIdentitifier>> lazyMachineIdentifiers;

		internal static readonly Dictionary<string, string> StoredPropertyNamesToInts = new Dictionary<string, string>
		{
			{
				"BiosSerialNumber",
				"0"
			},
			{
				"BiosUUID",
				"1"
			},
			{
				"ConfigVersion",
				"2"
			},
			{
				"DNSDomain",
				"3"
			},
			{
				"HardwareIdDate",
				"4"
			},
			{
				"MachineName",
				"5"
			},
			{
				"PersistedSelectedInterface",
				"6"
			},
			{
				"PersistedSelectedMACAddress",
				"7"
			},
			{
				"SelectedMACAddress",
				"8"
			}
		};

		private static readonly Lazy<PIIPropertyProcessor> LazyPiiPropertyProcessor = new Lazy<PIIPropertyProcessor>(() => new PIIPropertyProcessor());

		private static readonly object LazyPiiPropertyProcessorLockObject = new object();

		private INetworkInterfacesInformationProvider networkInterfacesInformationProvider;

		private Dictionary<string, MachineIdentitifier> MachineIdentifiers => lazyMachineIdentifiers.Value;

		private List<Exception> ExceptionsEncounteredObtainingHardwareId
		{
			get;
		} = new List<Exception>();


		private bool IsMachineStoreAccessible => lazyMachineStore.Value != null;

		internal static string DefaultStorageFileName => Path.Combine("Coding4Fun Visual Studio", "MachineStorage.dat");

		/// <summary>
		///     Gets the current config version. This value will come from the version of the dynammic telemetry rules.
		/// </summary>
		internal string ConfigVersion => MachineIdentityConfig.ConfigVersion;

		/// <summary>
		///     Gets the minimum valid config version. If this version is higher than the persisted version the hardware id
		///     will be invalidated.
		/// </summary>
		internal string MinValidConfigVersion => MachineIdentityConfig.MinValidConfigVersion;

		/// <summary>
		///     Gets a value indicating whether the persisted selected Mac address no longer existing on the machine should
		///     result in the hardwareId being invalidated.
		/// </summary>
		internal bool InvalidateOnPrimaryIdChange => MachineIdentityConfig.InvalidateOnPrimaryIdChange;

		internal string[] HardwareIdValues => MachineIdentityConfig.HardwareIdComponents;

		public string PersistedSelectedMACAddress => MachineIdentifiers["PersistedSelectedMACAddress"].CurrentStoredValue;

		public string PersistedSelectedInterface => MachineIdentifiers["PersistedSelectedInterface"].CurrentStoredValue;

		public bool PersistedIdWasInvalidated
		{
			get;
			protected set;
		}

		public string PersistedIdInvalidationReason
		{
			get;
			protected set;
		}

		public bool AnyValueChanged
		{
			get;
			protected set;
		}

		/// <summary>
		///     Gets the Persisted Id if it exists. Null if it does not exist.
		/// </summary>
		public string HardwareId => MachineIdentifiers["HardwareId"].CurrentSystemValue;

		/// <summary>
		///     Gets the local machines HostName
		/// </summary>
		public string MachineName => IpGlobalProperties.Value.HostName;

		/// <summary>
		///     Gets the local machines DNS Domain.
		/// </summary>
		public string DNSDomain => IpGlobalProperties.Value.DomainName;

		/// <summary>
		///     Gets the BIOS Serial Number of the local machine
		/// </summary>
		public abstract string BiosSerialNumber
		{
			get;
		}

		/// <summary>
		///     Gets the BIOS Universially Unique Identifier of the local machine
		/// </summary>
		public abstract Guid BiosUUID
		{
			get;
		}

		/// <summary>
		///     Gets the kind of Error that occured while parsing the BIOS firmware table, which is Success if no error was
		///     encountered.
		/// </summary>
		public abstract BiosFirmwareTableParserError BiosInformationError
		{
			get;
		}

		public DateTime? HardwareIdDate
		{
			get
			{
				if (!DateTime.TryParse(lazyMachineStore.Value.GetProperty(StoredPropertyNamesToInts["HardwareIdDate"]) as string, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime result))
				{
					return null;
				}
				return result;
			}
		}

		/// <summary>
		///     Gets the Selected MAC Address of the local machine from customized network interface prioritization logic.
		/// </summary>
		public string SelectedMACAddress => networkInterfacesInformationProvider.SelectedMACAddress;

		/// <summary>
		///     Gets a list of the local machine's Network interfaces ordered by customized prioritization logic.
		/// </summary>
		public List<NetworkInterfaceCardInformation> PrioritizedNetworkInterfaces => networkInterfacesInformationProvider.PrioritizedNetworkInterfaces;

		public TelemetryManifestMachineIdentityConfig MachineIdentityConfig
		{
			get;
			private set;
		}

		private string SelectedNetworkInterfaceDescription => PrioritizedNetworkInterfaces.FirstOrDefault((NetworkInterfaceCardInformation n) => n.SelectionRank == 0)?.Description;

		public event EventHandler<EventArgs> HardwareIdCalculationCompleted;

		/// <summary>
		///     Creates an instance of the IdentitiyInformatinoProvider
		/// </summary>
		protected IdentityInformationProvider(Func<IPersistentPropertyBag> createStore)
		{
			CodeContract.RequiresArgumentNotNull<Func<IPersistentPropertyBag>>(createStore, "createStore");
			lazyMachineStore = new Lazy<IPersistentPropertyBag>(createStore);
			lazyMachineIdentifiers = new Lazy<Dictionary<string, MachineIdentitifier>>(() => ConfigureIdentities());
		}

		private static string JoinIdentifiers(IEnumerable<MachineIdentitifier> ids, Func<MachineIdentitifier, string> transform)
		{
			string text = string.Join("|", ids.Select(transform));
			if (!(text == "||"))
			{
				return text;
			}
			return null;
		}

		public void GetHardwareIdWithCalculationCompletedEvent(Action<string> callback)
		{
			if (IsMachineStoreAccessible)
			{
				string hardwareId = HardwareId;
				if (!ExceptionsEncounteredObtainingHardwareId.Any())
				{
					callback(hardwareId);
				}
			}
			this.HardwareIdCalculationCompleted?.Invoke(this, EventArgs.Empty);
		}

		public void Initialize(TelemetryContext telemetryContext, ITelemetryScheduler contextScheduler, TelemetryManifestMachineIdentityConfig machineIdentityConfig)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryContext>(telemetryContext, "telemetryContext");
			MachineIdentityConfig = (machineIdentityConfig ?? TelemetryManifestMachineIdentityConfig.DefaultConfig);
			networkInterfacesInformationProvider = new NetworkInterfacesInformationProvider(MachineIdentityConfig.SmaRules);
			(contextScheduler ?? new TelemetryScheduler()).Schedule(delegate
			{
				GetHardwareIdWithCalculationCompletedEvent(delegate
				{
					telemetryContext.SharedProperties[IdentityPropertyProvider.HardwareIdPropertyName] = HardwareId;
				});
			});
		}

		/// <summary>
		///     Trigger collection of identity properties and return those values to be sent via telemetry.
		/// </summary>
		/// <returns>Properties to be sent in Identity Telemetry event</returns>
		public IEnumerable<KeyValuePair<string, object>> CollectIdentifiers(List<Exception> collectionExceptions)
		{
			yield return new KeyValuePair<string, object>("IsMachineStoreAccessible", IsMachineStoreAccessible);
			if (IsMachineStoreAccessible)
			{
				List<string> primaryIdValuesNoLongerExist = new List<string>();
				foreach (MachineIdentitifier id in MachineIdentifiers.Values)
				{
					yield return new KeyValuePair<string, object>(id.Name, (id.Type != MachineIdentitifier.IdType.Informational) ? ((object)new TelemetryPiiProperty(id.CurrentSystemValueUnhased)) : ((object)id.ValidValue));
					if (id.Type == MachineIdentitifier.IdType.PrimaryId && !string.IsNullOrWhiteSpace(id.LastPersistedValue) && !(id as MachinePrimaryIdentitifier).PrimaryIdStillExists)
					{
						primaryIdValuesNoLongerExist.Add(id.Name);
					}
					if (!id.IsLastPersistedValueValid)
					{
						if (!string.IsNullOrWhiteSpace(id.LastPersistedValue))
						{
							string key = id.Name + ".Last";
							yield return new KeyValuePair<string, object>(key, id.LastPersistedValue);
						}
						if (id.Type != MachineIdentitifier.IdType.PrimaryCombinedId)
						{
							id.StoreValidValue();
							AnyValueChanged = true;
						}
					}
				}
				if (primaryIdValuesNoLongerExist.Any())
				{
					yield return new KeyValuePair<string, object>("PrimaryIdValueNoLongerExists", string.Join(",", primaryIdValuesNoLongerExist));
				}
				yield return new KeyValuePair<string, object>("HardwareIdDate", HardwareIdDate.Value);
				yield return new KeyValuePair<string, object>("HardwareIdAge", (DateTime.UtcNow - HardwareIdDate.Value).TotalDays);
				try
				{
					lazyMachineStore.Value.Persist();
				}
				catch (Exception item)
				{
					collectionExceptions.Add(item);
				}
			}
		}

		public void SchedulePostPersistedSharedPropertyAndSendAnyFaults(TelemetrySession telemetrySession, ITelemetryScheduler scheduler)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			if (IsMachineStoreAccessible)
			{
				scheduler.Schedule(delegate
				{
					PostAnyFaultsGettingHardwareId(telemetrySession, telemetrySession.CancellationToken);
				}, telemetrySession.CancellationToken);
				scheduler.Schedule(delegate
				{
					PostPersistedSharedProperties(telemetrySession, telemetrySession.CancellationToken);
				}, telemetrySession.CancellationToken);
			}
		}

		private void PostAnyFaultsGettingHardwareId(TelemetrySession telemetrySession, CancellationToken cancellationToken)
		{
			foreach (Exception item in ExceptionsEncounteredObtainingHardwareId)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}
				telemetrySession.PostFault(HardwareIdEventFaultName, "ExceptionObtainingHardwareId", item);
			}
		}

		private void PostPersistedSharedProperties(TelemetrySession telemetrySession, CancellationToken cancellationToken)
		{
			if (!ExceptionsEncounteredObtainingHardwareId.Any())
			{
				string b = telemetrySession.GetPersistedSharedProperty(IdentityPropertyProvider.HardwareIdPropertyName) as string;
				string hardwareId = HardwareId;
				if (HardwareId != HardwareIdNotObtained && !string.Equals(hardwareId, b, StringComparison.Ordinal))
				{
					telemetrySession.SetPersistedSharedProperty(IdentityPropertyProvider.HardwareIdPropertyName, hardwareId);
				}
			}
		}

		private string GetHardwareIdWithInvalidation()
		{
			string text = HardwareIdNotObtained;
			try
			{
				text = MachineIdentifiers["HardwareId"].LastPersistedValue;
				if (!PersistedIdWasInvalidated && string.IsNullOrWhiteSpace(text))
				{
					PersistedIdWasInvalidated = true;
					PersistedIdInvalidationReason = HardwareIdNotPeristed;
				}
				if (!PersistedIdWasInvalidated)
				{
					foreach (MachineIdentitifier item2 in from name in HardwareIdValues
						select MachineIdentifiers[name] into id
						where id.Type == MachineIdentitifier.IdType.PrimaryImmutableId
						select id)
					{
						if (!PersistedIdWasInvalidated && !item2.IsLastPersistedValueValid)
						{
							PersistedIdWasInvalidated = true;
							PersistedIdInvalidationReason = string.Format(string.IsNullOrWhiteSpace(item2.LastPersistedValue) ? "Persisted identity '{0}' was not found." : "{0} change requires invalidation of persisted ID.", item2.Name);
						}
					}
				}
				MachineIdentitifier machineIdentitifier = MachineIdentifiers["ConfigVersion"];
				if (!PersistedIdWasInvalidated && string.Compare(machineIdentitifier.LastPersistedValue, MinValidConfigVersion, StringComparison.OrdinalIgnoreCase) < 0)
				{
					PersistedIdWasInvalidated = true;
					PersistedIdInvalidationReason = $"Persisted config version '{machineIdentitifier.LastPersistedValue}' is less than MinValidConfigVersion '{MinValidConfigVersion}'";
				}
				if (!PersistedIdWasInvalidated && InvalidateOnPrimaryIdChange)
				{
					foreach (MachineIdentitifier item3 in from name in HardwareIdValues
						select MachineIdentifiers[name] into id
						where id.Type == MachineIdentitifier.IdType.PrimaryId && !PersistedIdWasInvalidated && !id.IsLastPersistedValueValid
						select id)
					{
						PersistedIdWasInvalidated = true;
						PersistedIdInvalidationReason = string.Format(string.IsNullOrWhiteSpace(item3.LastPersistedValue) ? "Persisted identity '{0}' was not found." : "PrimaryId '{0}' value no longer found on machine.", item3.Name);
					}
				}
				if (PersistedIdWasInvalidated)
				{
					text = JoinIdentifiers(HardwareIdValues.Select((string idName) => MachineIdentifiers[idName]), (MachineIdentitifier id) => id.CurrentSystemValue);
					lazyMachineStore.Value.SetProperty(StoredPropertyNamesToInts["HardwareIdDate"], DateTime.UtcNow.ToString("o"));
					lazyMachineStore.Value.SetProperty(StoredPropertyNamesToInts["ConfigVersion"], ConfigVersion);
					string[] hardwareIdValues = HardwareIdValues;
					foreach (string key in hardwareIdValues)
					{
						MachineIdentifiers[key].StoreValidValue();
					}
				}
				lazyMachineStore.Value.Persist();
				return text;
			}
			catch (Exception item)
			{
				ExceptionsEncounteredObtainingHardwareId.Add(item);
				return text;
			}
		}

		internal static string FormatPropertyValue<T>(T value)
		{
			if (value == null)
			{
				return null;
			}
			if (!(typeof(T) == typeof(Guid)))
			{
				return value.ToString();
			}
			return ((Guid)(object)value).ToString("D");
		}

		private bool MACAddressStillExists(string mac)
		{
			lock (LazyPiiPropertyProcessorLockObject)
			{
				return PrioritizedNetworkInterfaces.Any((NetworkInterfaceCardInformation nic) => string.Equals(mac, LazyPiiPropertyProcessor.Value.ConvertToHashedValue(new TelemetryPiiProperty(nic.MacAddress))));
			}
		}

		private bool NetworkInterfaceStillExists(string interfaceDescription)
		{
			return PrioritizedNetworkInterfaces.Any((NetworkInterfaceCardInformation nic) => string.Equals(interfaceDescription, nic.Description, StringComparison.OrdinalIgnoreCase));
		}

		private Dictionary<string, MachineIdentitifier> ConfigureIdentities()
		{
			Dictionary<string, MachineIdentitifier> identifiersDict = new Dictionary<string, MachineIdentitifier>(StringComparer.OrdinalIgnoreCase)
			{
				{
					"MachineName",
					new MachineIdentitifier(lazyMachineStore, "MachineName", MachineIdentitifier.IdType.Secondary, () => MachineName)
				},
				{
					"DNSDomain",
					new MachineIdentitifier(lazyMachineStore, "DNSDomain", MachineIdentitifier.IdType.Secondary, () => DNSDomain)
				},
				{
					"SelectedMACAddress",
					new MachineIdentitifier(lazyMachineStore, "SelectedMACAddress", MachineIdentitifier.IdType.Secondary, () => SelectedMACAddress)
				},
				{
					"BiosUUID",
					new MachineIdentitifier(lazyMachineStore, "BiosUUID", MachineIdentitifier.IdType.PrimaryImmutableId, () => FormatPropertyValue(BiosUUID))
				},
				{
					"BiosSerialNumber",
					new MachineIdentitifier(lazyMachineStore, "BiosSerialNumber", MachineIdentitifier.IdType.PrimaryImmutableId, () => BiosSerialNumber)
				},
				{
					"ConfigVersion",
					new MachineIdentitifier(lazyMachineStore, "ConfigVersion", MachineIdentitifier.IdType.Informational, () => ConfigVersion, (string persistedConfigVersion) => string.CompareOrdinal(persistedConfigVersion, ConfigVersion) >= 0)
				},
				{
					"PersistedSelectedMACAddress",
					new MachinePrimaryIdentitifier(lazyMachineStore, "PersistedSelectedMACAddress", () => SelectedMACAddress, (string persistedSelectedMACAddress) => !InvalidateOnPrimaryIdChange, MACAddressStillExists)
				}
			};
			identifiersDict.Add("PersistedSelectedInterface", new MachineIdentitifier(lazyMachineStore, "PersistedSelectedInterface", MachineIdentitifier.IdType.Informational, () => SelectedNetworkInterfaceDescription, (string persistedInterface) => identifiersDict["PersistedSelectedMACAddress"].IsLastPersistedValueValid || object.Equals(persistedInterface, SelectedNetworkInterfaceDescription), null, NetworkInterfaceStillExists));
			identifiersDict.Add("HardwareId", new MachineComboIdentitifier(lazyMachineStore, "HardwareId", () => GetHardwareIdWithInvalidation(), () => HardwareIdValues.Select((string idName) => identifiersDict[idName])));
			return identifiersDict;
		}

		private static IPGlobalProperties InitializeIpGlobalProperties()
		{
			return IPGlobalProperties.GetIPGlobalProperties();
		}
	}
}
