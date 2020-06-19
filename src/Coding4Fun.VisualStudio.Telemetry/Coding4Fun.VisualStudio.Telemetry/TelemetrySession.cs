using Coding4Fun.VisualStudio.LocalLogger;
using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Represents one telemetry session that can post telemetry events
	/// and session properties.
	/// The API makes the best effort to deliver telemetry data to the server.
	/// The requests are queued and sent in batches.
	/// The events will be saved locally if they cannot be sent before the process exits.
	/// Sending will resume when the next application instance calls the telemetry API.
	/// Telemetry may be lost in case of process hang or fatal crash or no Internet
	/// connection for 30 days.
	/// All methods are thread-safe and return immediately.
	/// </summary>
	public class TelemetrySession : TelemetryDisposableObject
	{
		/// <summary>
		/// Constant for value which is not available
		/// </summary>
		public const string ValueNotAvailable = "Unknown";

		/// <summary>
		/// Default context name
		/// </summary>
		public const string DefaultContextName = "Default";

		/// <summary>
		/// Timeout for the acquire "write" permission for the block
		/// posting custom events during disposing contexts from the
		/// multiple threads
		/// 1 second should be enough to finish all pending telemetry
		/// </summary>
		private const int WaitForPendingPostingTimeout = 1000;

		private const int DisposingIsStarted = 1;

		private const int DisposingNotStarted = 0;

		private readonly TelemetrySessionInitializer sessionInitializer;

		private readonly TelemetryPropertyBags.Concurrent<TelemetryContext> sessionContexts = new TelemetryPropertyBags.Concurrent<TelemetryContext>();

		private readonly LinkedList<TelemetryContext> sessionContextStack = new LinkedList<TelemetryContext>();

		private readonly object startedLock = new object();

		private readonly object initializedLock = new object();

		private readonly TelemetryPropertyBags.Concurrent<TelemetryPropertyBag> propertyBagDictionary = new TelemetryPropertyBags.Concurrent<TelemetryPropertyBag>();

		private readonly IMachineInformationProvider machineInformationProvider;

		private readonly IMACInformationProvider macInformationProvider;

		private readonly IUserInformationProvider userInformationProvider;

		private readonly TelemetryContext defaultContext;

		private readonly TelemetrySessionSettings sessionSettings;

		private readonly IContextPropertyManager defaultContextPropertyManager;

		private readonly EventProcessorChannel eventProcessorChannel;

		private readonly IEnumerable<IChannelValidator> channelValidators;

		private readonly Lazy<ITelemetryManifestManager> telemetryManifestManager;

		private readonly IPersistentPropertyBag persistentPropertyBag;

		private readonly IPersistentPropertyBag persistentSharedProperties;

		private readonly ITelemetryScheduler contextScheduler;

		private readonly CancellationTokenSource cancellationTokenSource;

		private readonly IDiagnosticTelemetry diagnosticTelemetry;

		private readonly IIdentityTelemetry identityTelemetry;

		private readonly ITelemetryOptinStatusReader optinStatusReader;

		private readonly HashSet<string> defaultSessionChannelsId = new HashSet<string>();

		private readonly bool isSessionCloned;

		/// <summary>
		/// Protects from posting custom events when default context is about being disposed
		/// </summary>
		private readonly ReaderWriterLockSlim customEventPostProtection = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		private TelemetryBufferChannel telemetryBufferChannel;

		private WatsonSessionChannel watsonSessionChannel;

		private Stopwatch disposeLatencyTimer;

		private int numberOfDroppedEventsInDisposing;

		private bool isSessionStarted;

		/// <summary>
		/// Until initialized, events are buffered
		/// </summary>
		private bool isInitialized;

		private bool isManifestCompleted;

		private bool isMACAddressCompleted;

		private bool isHardwareIdCompleted;

		private int startedDisposing;

		/// <summary>
		/// for TelemetryNotificationService
		/// </summary>
		internal EventHandler<TelemetryTestChannelEventArgs> RawTelemetryEventReceived;

		/// <summary>
		/// Gets the session Id.
		/// </summary>
		public string SessionId => sessionSettings.Id;

		/// <summary>
		/// Gets or sets a value indicating whether user is opted in
		/// </summary>
		public bool IsOptedIn
		{
			get
			{
				return sessionSettings.IsOptedIn;
			}
			set
			{
				sessionSettings.IsOptedIn = value;
				SetOptedInProperty();
			}
		}

		/// <summary>
		/// Gets or sets telemetry host name, it affects on manifest file location
		/// </summary>
		public string HostName
		{
			get
			{
				return sessionSettings.HostName;
			}
			set
			{
				CodeContract.RequiresArgumentNotNullAndNotEmpty(value, "value");
				if (!ValidateHostName(value))
				{
					throw new ArgumentException("hostname must be more than 0 characters length, less than 65 characters length and consist of alphanumeric and/or characters '_', '-', ' ', '+'");
				}
				if (sessionSettings.CanOverrideHostName)
				{
					sessionSettings.HostName = value;
				}
			}
		}

		/// <summary>
		/// Gets the Telemetry Session's CancellationToken
		/// </summary>
		public CancellationToken CancellationToken => cancellationTokenSource.Token;

		/// <summary>
		/// Gets or sets application id for SQM
		/// </summary>
		[CLSCompliant(false)]
		public uint AppId
		{
			get
			{
				return sessionSettings.AppId;
			}
			set
			{
				if (value < 1000)
				{
					throw new ArgumentException("AppId for the Telemetry API starts at 1000");
				}
				if (sessionSettings.CanOverrideAppId)
				{
					sessionSettings.AppId = value;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether current session can collect PII information based on the answer
		/// from the User Information Manager
		/// </summary>
		/// <returns></returns>
		public bool CanCollectPrivateInformation => userInformationProvider.CanCollectPrivateInformation;

		/// <summary>
		/// Gets a value indicating whether current session belongs to internal user thus can collect information based on the answer
		/// from the User Information Manager
		/// </summary>
		/// <returns></returns>
		public bool IsUserMicrosoftInternal => userInformationProvider.IsUserMicrosoftInternal;

		/// <summary>
		/// Gets/Sets calculated sampling from the manifest file
		/// </summary>
		public string CalculatedSamplings
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets machine id (SQM Machine Id)
		/// </summary>
		public Guid MachineId => machineInformationProvider.MachineId;

		/// <summary>
		/// Gets user id (SQM User Id)
		/// </summary>
		public Guid UserId => userInformationProvider.UserId;

		/// <summary>
		/// Gets the MAC address hash. If the hash is not found in
		/// local storage, a 0 hash will be returned and the hashing
		/// process will be invoked
		/// </summary>
		public string MacAddressHash => macInformationProvider.GetMACAddressHash();

		/// <summary>
		/// Gets TimeSinceSessionStart in case if customers want to use it to calculate custom delays based on the same
		/// algorithm Telemetry does.
		/// </summary>
		public long TimeSinceSessionStart => (long)new TimeSpan(DateTime.UtcNow.Ticks - ProcessStartTime).TotalMilliseconds;

		/// <summary>
		/// Sets the action that is fired after initial events have made it through to
		/// channels.
		/// </summary>
		internal Action InitializedAction
		{
			set
			{
				eventProcessorChannel.InitializedAction = value;
			}
		}

		internal EventProcessor EventProcessor
		{
			get;
		}

		internal TelemetryContext DefaultContext => defaultContext;

		internal bool IsSessionCloned => isSessionCloned;

		internal ITelemetryManifestManager ManifestManager => telemetryManifestManager.Value;

		/// <summary>
		/// Gets the process start time for this session
		/// </summary>
		internal long ProcessStartTime
		{
			get
			{
				if (sessionSettings != null)
				{
					return sessionSettings.ProcessStartTime;
				}
				return 0L;
			}
		}

		/// <summary>
		/// Creates a new telemetry session based on a
		/// serialized string of a TelemetrySessionSettings instance.
		/// Use the SerializeSettings method of an instance of TelemetrySession
		/// to get such a string. This enables a process to post events
		/// to a telemetry session from another process.
		/// Use VSTelemetryService.DefaultSession to access the application session.
		/// </summary>
		/// <param name="serializedSession"></param>
		public TelemetrySession(string serializedSession)
			: this(TelemetrySessionSettings.Parse(serializedSession))
		{
		}

		/// <summary>
		/// Create TelemetrySession object from serialized session settings and
		/// use the session settings to setup initializer
		/// </summary>
		/// <param name="telemetrySessionSettings"></param>
		internal TelemetrySession(TelemetrySessionSettings telemetrySessionSettings)
			: this(telemetrySessionSettings, true, TelemetrySessionInitializer.FromSessionSettings(telemetrySessionSettings))
		{
		}

		/// <summary>
		/// Create TelemetrySession object from serialized session settings and
		/// using custom session initializer object
		/// </summary>
		/// <param name="serializedSession"></param>
		/// <param name="initializerObject"></param>
		internal TelemetrySession(string serializedSession, TelemetrySessionInitializer initializerObject)
			: this(TelemetrySessionSettings.Parse(serializedSession), true, initializerObject)
		{
		}

		/// <summary>
		/// Start session means create default context for the session and
		/// add private information properties if it is allowed.
		/// </summary>
		public void Start()
		{
			Start(false);
		}

		/// <summary>
		/// Reads and set VS OptIn status.
		/// </summary>
		/// <param name="productVersion">VS product version so it is possible to build settings path</param>
		public void UseVsIsOptedIn(string productVersion)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(productVersion, "productVersion");
			IsOptedIn = optinStatusReader.ReadIsOptedInStatus(productVersion);
		}

		/// <summary>
		/// Reads and set VS OptIn status.
		/// Calculate IsOptedIn status based on OptedIn status from all installed versions of VS.
		/// If all found OptedIn statuses are true we return true, otherwise we return false.
		/// </summary>
		public void UseVsIsOptedIn()
		{
			IsOptedIn = optinStatusReader.ReadIsOptedInStatus(this);
		}

		/// <summary>
		/// Create context with the specific name.
		/// </summary>
		/// <param name="contextName"></param>
		/// <returns></returns>
		public TelemetryContext CreateContext(string contextName)
		{
			RequiresNotDisposed();
			RequiresNotDefaultContextName(contextName);
			return new TelemetryContext(contextName, this, contextScheduler);
		}

		/// <summary>
		/// Gets a context by the given name.
		/// </summary>
		/// <param name="contextName"></param>
		/// <returns>null if no context with such name.</returns>
		public TelemetryContext GetContext(string contextName)
		{
			RequiresNotDefaultContextName(contextName);
			return DictionaryExtensions.GetOrDefault<string, TelemetryContext>((IDictionary<string, TelemetryContext>)sessionContexts, contextName);
		}

		/// <summary>
		/// Queues a telemetry event to be posted to a server.
		/// Choose this method for simplicity if the name is the only event property.
		/// You should consider choosing PostUserTask, PostOperation, PostFault or PostAsset.
		/// These will enable a richer telemetry experience with additional insights provided by Visual Studio Data Model.
		/// If your data point doesn't align with any VS Data Model entity, please don't force any association and continue to use this method.
		/// If you have any questions regarding VS Data Model, please email VS Data Model Crew (vsdmcrew@microsoft.com).
		/// </summary>
		/// <param name="eventName">Event name that is unique,
		/// not null and not empty.</param>
		public void PostEvent(string eventName)
		{
			if (!base.IsDisposed)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent(eventName);
				PostEvent(telemetryEvent);
			}
		}

		/// <summary>
		/// Queues a telemetry event to be posted to a server.
		/// Choose this method for flexibility.
		/// You should consider choosing PostUserTask, PostOperation, PostFault or PostAsset.
		/// These will enable a richer telemetry experience with additional insights provided by Visual Studio Data Model.
		/// If your data point doesn't align with any VS Data Model entity, please don't force any association and continue to use this method.
		/// If you have any questions regarding VS Data Model, please email VS Data Model Crew (vsdmcrew@microsoft.com).
		/// </summary>
		/// <param name="telemetryEvent">A telemetry event that is ready to be posted.</param>
		public void PostEvent(TelemetryEvent telemetryEvent)
		{
			if (base.IsDisposed)
			{
				return;
			}
			bool flag = true;
			FaultEvent faultEvent = telemetryEvent as FaultEvent;
			if (faultEvent != null && watsonSessionChannel != null)
			{
				watsonSessionChannel.PostEvent(faultEvent);
				flag = faultEvent.PostThisEventToTelemetry;
			}
			if (flag)
			{
				if (!customEventPostProtection.TryEnterReadLock(0))
				{
					numberOfDroppedEventsInDisposing++;
					return;
				}
				ValidateEvent(telemetryEvent);
				TelemetryContext.ValidateEvent(telemetryEvent);
				PostValidatedEvent(telemetryEvent);
				customEventPostProtection.ExitReadLock();
			}
			TelemetryActivity telemetryActivity = telemetryEvent as TelemetryActivity;
			if (telemetryActivity != null)
			{
				TelemetryService.TelemetryEventSource.WriteActivityPostEvent(telemetryActivity, this);
			}
			else
			{
				TelemetryService.TelemetryEventSource.WriteTelemetryPostEvent(telemetryEvent, this);
			}
		}

		/// <summary>
		/// Queues an update to for a session-wide property.
		/// You may want to consider <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySettingProperty" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryMetricProperty" />
		/// These will enable a richer telemetry experience with additional insights provided by Visual Studio Data Model.
		/// If you have any questions regarding VS Data Model, please email VS Data Model Crew (vsdmcrew@microsoft.com).
		/// </summary>
		/// <param name="propertyName">Session property name that is unique,
		/// not null and not empty.</param>
		/// <param name="propertyValue"> Any object that represents a property value.
		/// Telemetry channels must use value.ToString(CultureInfo.InvariantCulture)
		/// to send the value to a server as a string.</param>
		public void PostProperty(string propertyName, object propertyValue)
		{
			if (!base.IsDisposed)
			{
				defaultContext.PostProperty(propertyName, propertyValue);
			}
		}

		/// <summary>
		/// Add new session channel to the list of active channels.
		/// </summary>
		/// <param name="sessionChannel"></param>
		public void AddSessionChannel(ISessionChannel sessionChannel)
		{
			RequiresNotDisposed();
			CodeContract.RequiresArgumentNotNull<ISessionChannel>(sessionChannel, "sessionChannel");
			if (IsValidChannel(sessionChannel))
			{
				EventProcessor.AddChannel(sessionChannel);
				if ((sessionChannel.Properties & ChannelProperties.Default) == ChannelProperties.Default)
				{
					defaultSessionChannelsId.Add(sessionChannel.ChannelId);
				}
			}
		}

		/// <summary>
		/// Serializes the TelemetrySessionSettings object.
		/// </summary>
		/// <returns></returns>
		public string SerializeSettings()
		{
			RequiresNotDisposed();
			sessionSettings.UserId = UserId;
			return sessionSettings.ToString();
		}

		/// <summary>
		/// Set shared property for the session means set shared property for the default context
		/// You may want to consider <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySettingProperty" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryMetricProperty" />
		/// These will enable a richer telemetry experience with additional insights provided by Visual Studio Data Model.
		/// If you have any questions regarding VS Data Model, please email VS Data Model Crew (vsdmcrew@microsoft.com).
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		public void SetSharedProperty(string propertyName, object propertyValue)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(propertyName, "propertyName");
				defaultContext.SharedProperties[propertyName] = propertyValue;
			}
		}

		/// <summary>
		/// Remove shared property for the session means remove shared property from the default context
		/// </summary>
		/// <param name="propertyName"></param>
		public void RemoveSharedProperty(string propertyName)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(propertyName, "propertyName");
				defaultContext.SharedProperties.Remove(propertyName);
			}
		}

		/// <summary>
		/// Get shared property by name.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>null if no shared property with such name.</returns>
		public dynamic GetSharedProperty(string propertyName)
		{
			return DictionaryExtensions.GetOrDefault<string, object>(defaultContext.SharedProperties, propertyName);
		}

		/// <summary>
		/// Set persisted shared property for the session means set shared property for the default context for
		/// this session and any future sessions on the machine.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		public void SetPersistedSharedProperty(string propertyName, string propertyValue)
		{
			SetPersistedSharedProperty(propertyName, propertyValue, delegate
			{
				persistentSharedProperties.SetProperty(propertyName, propertyValue);
			});
		}

		/// <summary>
		/// Set persisted shared property for the session means set shared property for the default context for
		/// this session and any future sessions on the machine.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="propertyValue"></param>
		public void SetPersistedSharedProperty(string propertyName, double propertyValue)
		{
			SetPersistedSharedProperty(propertyName, propertyValue, delegate
			{
				persistentSharedProperties.SetProperty(propertyName, propertyValue);
			});
		}

		/// <summary>
		/// Remove persisted shared property for all sessions means remove shared property from this default context,
		/// and for any future sessions on the machine.
		/// </summary>
		/// <param name="propertyName"></param>
		public void RemovePersistedSharedProperty(string propertyName)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(propertyName, "propertyName");
				dynamic persistedSharedProperty = GetPersistedSharedProperty(propertyName);
				persistentSharedProperties.RemoveProperty(propertyName);
				defaultContext.SharedProperties.Remove(propertyName);
				if (persistedSharedProperty != null)
				{
					TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/PersistedSharedProperty/Remove");
					telemetryEvent.Properties["VS.TelemetryApi.PersistedSharedProperty.Name"] = propertyName;
					PostEvent(telemetryEvent);
				}
			}
		}

		/// <summary>
		/// Get persisted shared property by name.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>null if no shared property with such name.</returns>
		public dynamic GetPersistedSharedProperty(string propertyName)
		{
			return persistentSharedProperties.GetProperty(propertyName);
		}

		/// <summary>
		/// Register the given property bag.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="propertyBag"></param>
		public void RegisterPropertyBag(string name, TelemetryPropertyBag propertyBag)
		{
			CodeContract.RequiresArgumentNotNull<string>(name, "name");
			propertyBagDictionary[name] = propertyBag;
		}

		/// <summary>
		/// Unregister a property bag with the given name.
		/// </summary>
		/// <param name="name"></param>
		public void UnregisterPropertyBag(string name)
		{
			DictionaryExtensions.Remove<string, TelemetryPropertyBag>((ConcurrentDictionary<string, TelemetryPropertyBag>)propertyBagDictionary, name);
		}

		/// <summary>
		/// Gets a property bag by the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns>null if no property bag with such name.</returns>
		public TelemetryPropertyBag GetPropertyBag(string name)
		{
			return DictionaryExtensions.GetOrDefault<string, TelemetryPropertyBag>((IDictionary<string, TelemetryPropertyBag>)propertyBagDictionary, name);
		}

		/// <summary>
		/// Asynchronously dispose and try to send all telemetry over the network.
		/// </summary>
		/// <returns>Task to wait on</returns>
		public async Task DisposeToNetworkAsync(CancellationToken token)
		{
			if (DisposeStart())
			{
				await eventProcessorChannel.DisposeAndTransmitAsync(token).ConfigureAwait(false);
				DisposeEnd();
			}
		}

		/// <summary>
		/// Start session means create default context for the session and
		/// add private information properties if it is allowed.
		/// </summary>
		/// <param name="checkPendingAsimovEvents">Do we need to check whether
		/// pending events exists and explicitly start Vortex channel</param>
		internal void Start(bool checkPendingAsimovEvents)
		{
			if (!isSessionStarted)
			{
				lock (startedLock)
				{
					if (!isSessionStarted)
					{
						defaultContextPropertyManager.AddDefaultContextProperties(DefaultContext);
						LocalFileLoggerService.Default.Enabled = sessionInitializer.InternalSettings.IsLocalLoggerEnabled();
						InitializeWithDefaultChannels(checkPendingAsimovEvents);
						if (!isSessionCloned)
						{
							defaultContextPropertyManager.PostDefaultContextProperties(DefaultContext);
						}
						SetOptedInProperty();
						isSessionStarted = true;
					}
				}
			}
		}

		/// <summary>
		/// Creates a new default telemetry session.
		/// </summary>
		/// <returns></returns>
		internal static TelemetrySession Create()
		{
			return Create(TelemetrySessionInitializer.Default);
		}

		/// <summary>
		/// Creates a new telemetry session from the given initializer.
		/// </summary>
		/// <param name="initializerObject"></param>
		/// <returns></returns>
		internal static TelemetrySession Create(TelemetrySessionInitializer initializerObject)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySessionInitializer>(initializerObject, "initializerObject");
			if (!TelemetrySessionSettings.IsSessionIdValid(initializerObject.SessionId))
			{
				throw new ArgumentException("Session ID in sessionSettings is not valid", "sessionSettings");
			}
			return new TelemetrySession(new TelemetrySessionSettings(initializerObject.SessionId, initializerObject.InternalSettings, initializerObject.AppInsightsInstrumentationKey, initializerObject.AsimovInstrumentationKey, initializerObject.ProcessCreationTime), false, initializerObject);
		}

		internal bool Equals(TelemetrySession other)
		{
			return sessionSettings.Equals(other.sessionSettings);
		}

		internal void PostValidatedEvent(TelemetryEvent telemetryEvent)
		{
			if (!base.IsDisposed)
			{
				TelemetryEvent telemetryEvent2 = telemetryEvent.BuildChannelEvent(ProcessStartTime, SessionId);
				AddContextProperties(telemetryEvent2);
				telemetryEvent2.PostTimestamp = DateTimeOffset.Now;
				RawTelemetryEventReceived?.Invoke(this, new TelemetryTestChannelEventArgs
				{
					Event = telemetryEvent2
				});
				if (!isInitialized)
				{
					telemetryBufferChannel.PostEvent(telemetryEvent2);
				}
				else
				{
					PostProcessedEvent(telemetryEvent2);
				}
			}
		}

		/// <summary>
		/// Add context to the contexts list.
		/// Context list is necessary to add shared properties to each event.
		/// </summary>
		/// <param name="telemetryContext"></param>
		internal void AddContext(TelemetryContext telemetryContext)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNull<TelemetryContext>(telemetryContext, "telemetryContext");
				string contextName = telemetryContext.ContextName;
				if (!sessionContexts.TryAdd(contextName, telemetryContext))
				{
					throw new ArgumentException("a context with name '" + contextName + "' already exist.");
				}
				lock (sessionContextStack)
				{
					sessionContextStack.AddLast(telemetryContext);
				}
			}
		}

		/// <summary>
		/// Remove context from the contexts list.
		/// </summary>
		/// <param name="telemetryContext"></param>
		internal void RemoveContext(TelemetryContext telemetryContext)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNull<TelemetryContext>(telemetryContext, "telemetryContext");
				DictionaryExtensions.Remove<string, TelemetryContext>((ConcurrentDictionary<string, TelemetryContext>)sessionContexts, telemetryContext.ContextName);
				lock (sessionContextStack)
				{
					sessionContextStack.Remove(telemetryContext);
				}
			}
		}

		/// <summary>
		/// Add new session channels for the current session
		/// </summary>
		/// <param name="channels"></param>
		internal void AddSessionChannels(IEnumerable<ISessionChannel> channels)
		{
			CodeContract.RequiresArgumentNotNull<IEnumerable<ISessionChannel>>(channels, "channels");
			foreach (ISessionChannel channel in channels)
			{
				AddSessionChannel(channel);
			}
		}

		/// <summary>
		/// Protected implementation of Dispose pattern.
		/// </summary>
		protected override void DisposeManagedResources()
		{
			if (DisposeStart())
			{
				eventProcessorChannel.Dispose();
				DisposeEnd();
			}
		}

		/// <summary>
		/// Start synchronous dispose
		/// </summary>
		/// <returns>can we continue with dispose or not</returns>
		private bool DisposeStart()
		{
			if (Interlocked.CompareExchange(ref startedDisposing, 1, 0) == 1)
			{
				return false;
			}
			disposeLatencyTimer = new Stopwatch();
			disposeLatencyTimer.Start();
			if (!isInitialized)
			{
				if (!isSessionStarted)
				{
					Start();
				}
				if (!ManifestManager.ForceReadManifest())
				{
					TelemetryManifestUpdateStatus(this, new TelemetryManifestEventArgs(null));
				}
				MACAddressHashCalculationCompleted(this, EventArgs.Empty);
				HardwareIdCalculationCompleted(this, EventArgs.Empty);
			}
			macInformationProvider.MACAddressHashCalculationCompleted -= MACAddressHashCalculationCompleted;
			identityTelemetry.IdentityInformationProvider.HardwareIdCalculationCompleted -= HardwareIdCalculationCompleted;
			ManifestManager.UpdateTelemetryManifestStatusEvent -= TelemetryManifestUpdateStatus;
			ManifestManager.Dispose();
			cancellationTokenSource.Cancel();
			defaultContextPropertyManager.Dispose();
			EventProcessor.PostDiagnosticInformationIfNeeded();
			customEventPostProtection.TryEnterWriteLock(1000);
			List<TelemetryContext> list;
			lock (sessionContextStack)
			{
				list = sessionContextStack.Reverse().ToList();
			}
			foreach (TelemetryContext item in list)
			{
				item.Dispose();
			}
			return true;
		}

		/// <summary>
		/// End synchronous dispose
		/// </summary>
		private void DisposeEnd()
		{
			lock (startedLock)
			{
				isSessionStarted = false;
			}
			persistentPropertyBag.SetProperty("VS.TelemetryApi.DroppedEventsDuringDisposing", numberOfDroppedEventsInDisposing);
			persistentPropertyBag.SetProperty("VS.TelemetryApi.TotalDisposeLatency", (int)disposeLatencyTimer.ElapsedMilliseconds);
		}

		private bool IsValidChannel(ISessionChannel sessionChannel)
		{
			foreach (IChannelValidator channelValidator in channelValidators)
			{
				if (!channelValidator.IsValid(sessionChannel))
				{
					return false;
				}
			}
			return true;
		}

		private static void RequiresNotDefaultContextName(string contextName)
		{
			if (TelemetryPropertyBags.KeyComparer.Equals(contextName, "Default"))
			{
				throw new ArgumentException("attempt to create context with reserved 'Default' name");
			}
		}

		/// <summary>
		/// Flush processed event.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		private void PostProcessedEvent(TelemetryEvent telemetryEvent)
		{
			if (!base.IsDisposed)
			{
				eventProcessorChannel.PostEvent(telemetryEvent);
			}
		}

		/// <summary>
		/// Set up telemetry session properties
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="initializerObject"></param>
		/// <param name="isCloned"></param>
		private TelemetrySession(TelemetrySessionSettings settings, bool isCloned, TelemetrySessionInitializer initializerObject)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySessionInitializer>(initializerObject, "initializerObject");
			initializerObject.Validate();
			initializerObject.AppInsightsInstrumentationKey = settings.AppInsightsInstrumentationKey;
			initializerObject.AsimovInstrumentationKey = settings.AsimovInstrumentationKey;
			TelemetryService.EnsureEtwProviderInitialized();
			sessionInitializer = initializerObject;
			sessionSettings = settings;
			isSessionCloned = isCloned;
			cancellationTokenSource = initializerObject.CancellationTokenSource;
			diagnosticTelemetry = initializerObject.DiagnosticTelemetry;
			identityTelemetry = initializerObject.IdentityTelemetry;
			optinStatusReader = initializerObject.OptinStatusReader;
			channelValidators = initializerObject.ChannelValidators;
			telemetryBufferChannel = new TelemetryBufferChannel();
			initializerObject.WatsonSessionChannelBuilder.Build(this);
			WatsonSessionChannel sessionChannel = initializerObject.WatsonSessionChannelBuilder.WatsonSessionChannel;
			if (IsValidChannel(sessionChannel))
			{
				watsonSessionChannel = sessionChannel;
			}
			machineInformationProvider = initializerObject.MachineInformationProvider;
			macInformationProvider = initializerObject.MACInformationProvider;
			userInformationProvider = initializerObject.UserInformationProvider;
			defaultContextPropertyManager = initializerObject.DefaultContextPropertyManager;
			persistentPropertyBag = initializerObject.PersistentPropertyBag;
			initializerObject.EventProcessorChannelBuilder.Build(this);
			persistentSharedProperties = initializerObject.PersistentSharedProperties;
			EventProcessor = initializerObject.EventProcessorChannelBuilder.EventProcessor;
			if (initializerObject.CustomActionToAdd != null)
			{
				foreach (IEventProcessorAction item in initializerObject.CustomActionToAdd)
				{
					EventProcessor.AddCustomAction(item);
				}
			}
			eventProcessorChannel = initializerObject.EventProcessorChannelBuilder.EventProcessorChannel;
			telemetryManifestManager = new Lazy<ITelemetryManifestManager>(() => initializerObject.TelemetryManifestManagerBuilder.Build(this));
			contextScheduler = initializerObject.ContextScheduler;
			if (initializerObject.ChannelsToAdd != null)
			{
				AddSessionChannels(initializerObject.ChannelsToAdd);
			}
			defaultContext = CreateDefaultContext();
			macInformationProvider.MACAddressHashCalculationCompleted += MACAddressHashCalculationCompleted;
			identityTelemetry.IdentityInformationProvider.HardwareIdCalculationCompleted += HardwareIdCalculationCompleted;
		}

		/// <summary>
		/// Create a default context with PII properties set if we can collect it.
		/// </summary>
		/// <returns></returns>
		private TelemetryContext CreateDefaultContext()
		{
			return new TelemetryContext("Default", this, contextScheduler, isSessionCloned, defaultContextPropertyManager.AddRealtimeDefaultContextProperties);
		}

		/// <summary>
		/// Passes default construction of channels and UserInformationManager to Initialize method
		/// </summary>
		/// <param name="checkPendingAsimovEvents">Do we need to check whether
		/// pending events exists and explicitly start Vortex channel</param>
		private void InitializeWithDefaultChannels(bool checkPendingAsimovEvents)
		{
			EventProcessor.CurrentManifest = TelemetryManifest.BuildDefaultManifest();
			ManifestManager.UpdateTelemetryManifestStatusEvent += TelemetryManifestUpdateStatus;
			AddSessionChannels(sessionInitializer.CreateSessionChannels(this, checkPendingAsimovEvents));
			AddSessionChannel(GlobalTelemetryTestChannel.Instance);
			ManifestManager.Start(HostName, startedDisposing == 1);
		}

		private void SetOptedInProperty()
		{
			SetSharedProperty("VS.Core.User.IsOptedIn", sessionSettings.IsOptedIn);
		}

		/// <summary>
		/// Add 3 Internal datapoints in case we are allowed to do it.
		/// </summary>
		private void SetInternalInformationProperties(TelemetryManifest telemetryManifest)
		{
			SetInternalUserNameIfApplicable(telemetryManifest);
			SetInternalComputerNameIfApplicable();
			SetInternalDomainNameIfApplicable();
		}

		private void SetInternalUserNameIfApplicable(TelemetryManifest telemetryManifest)
		{
			if ((IsUserMicrosoftInternal && telemetryManifest.ShouldSendAliasForAllInternalUsers) || CanCollectPrivateInformation)
			{
				DefaultContext.SharedProperties["VS.Core.Internal.UserName"] = Guard(() => Environment.UserName, "Unknown");
			}
		}

		private void SetInternalComputerNameIfApplicable()
		{
			if (CanCollectPrivateInformation)
			{
				DefaultContext.SharedProperties["VS.Core.Internal.ComputerName"] = Guard(() => Environment.MachineName, "Unknown");
			}
		}

		private void SetInternalDomainNameIfApplicable()
		{
			if (IsUserMicrosoftInternal)
			{
				DefaultContext.SharedProperties["VS.Core.Internal.UserDomainName"] = Guard(() => Environment.UserDomainName, "Unknown");
			}
		}

		/// <summary>
		/// Add context properties to the event.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		private void AddContextProperties(TelemetryEvent telemetryEvent)
		{
			if (!base.IsDisposed)
			{
				foreach (KeyValuePair<string, TelemetryContext> sessionContext in sessionContexts)
				{
					sessionContext.Value.ProcessEvent(telemetryEvent);
				}
				foreach (KeyValuePair<string, TelemetryContext> sessionContext2 in sessionContexts)
				{
					sessionContext2.Value.ProcessEventRealtime(telemetryEvent);
				}
			}
		}

		private void SetPersistedSharedProperty(string propertyName, object propertyValue, Action addToBagAction)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(propertyName, "propertyName");
				dynamic persistedSharedProperty = GetPersistedSharedProperty(propertyName);
				addToBagAction();
				defaultContext.SharedProperties[propertyName] = propertyValue;
				if (persistedSharedProperty == null || !persistedSharedProperty.Equals(propertyValue))
				{
					TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/PersistedSharedProperty/Set");
					telemetryEvent.Properties["VS.TelemetryApi.PersistedSharedProperty.Name"] = propertyName;
					telemetryEvent.Properties["VS.TelemetryApi.PersistedSharedProperty.IsChangedValue"] = (object)(persistedSharedProperty != null);
					PostEvent(telemetryEvent);
				}
			}
		}

		/// <summary>
		/// Guard function returns value in case it is available.
		/// In case of fail it returns defaultValue
		/// </summary>
		/// <param name="provider">provider function for getting value</param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		internal static string Guard(Func<string> provider, string defaultValue)
		{
			try
			{
				string text = provider();
				if (string.IsNullOrEmpty(text))
				{
					text = defaultValue;
				}
				return text;
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Validate event. Ensure that event regular properties doesn't contain Reserved. prefix
		/// </summary>
		/// <param name="telemetryEvent"></param>
		internal static void ValidateEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			foreach (KeyValuePair<string, object> property in telemetryEvent.Properties)
			{
				if (property.Key.StartsWith("Reserved.", StringComparison.Ordinal) && TelemetryEvent.IsPropertyNameReserved(property.Key))
				{
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "property '{0}' has reserved prefix 'Reserved'", new object[1]
					{
						property.Key
					}));
				}
			}
		}

		/// <summary>
		/// Handles the TelemetryManifestUpdateStatus event from the downloader by giving the
		/// loaded TelemetryManifest to the EventProcessor in case of the success.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TelemetryManifestUpdateStatus(object sender, TelemetryManifestEventArgs e)
		{
			if (!base.IsDisposed)
			{
				if (e.IsSuccess)
				{
					TelemetryManifest telemetryManifest = e.TelemetryManifest;
					CalculatedSamplings = telemetryManifest.CalculateAllSamplings(this);
					EventProcessor.CurrentManifest = telemetryManifest;
				}
				else
				{
					CalculatedSamplings = "e.IsSuccess == false";
				}
				if (!isManifestCompleted)
				{
					identityTelemetry.IdentityInformationProvider.Initialize(defaultContext, contextScheduler, e.TelemetryManifest?.MachineIdentityConfig);
					SetInternalInformationProperties(EventProcessor.CurrentManifest);
				}
				isManifestCompleted = true;
				InitializeSession();
			}
		}

		/// <summary>
		/// Handles the MACAddressHashCalculationCompleted event from the MACInformationProvider
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MACAddressHashCalculationCompleted(object sender, EventArgs e)
		{
			isMACAddressCompleted = true;
			InitializeSession();
		}

		private void HardwareIdCalculationCompleted(object sender, EventArgs e)
		{
			isHardwareIdCompleted = true;
			InitializeSession();
			identityTelemetry.IdentityInformationProvider.HardwareIdCalculationCompleted -= HardwareIdCalculationCompleted;
		}

		private void InitializeSession()
		{
			if (!isInitialized)
			{
				lock (initializedLock)
				{
					if (!isInitialized && isManifestCompleted && isMACAddressCompleted && isHardwareIdCompleted)
					{
						List<KeyValuePair<string, object>> list = persistentPropertyBag.GetAllProperties().ToList();
						list.Add(new KeyValuePair<string, object>("VS.Core.Session.IsCloned", isSessionCloned));
						list.Add(new KeyValuePair<string, object>("VS.TelemetryApi.Session.IsShort", startedDisposing));
						list.Add(new KeyValuePair<string, object>("VS.TelemetryApi.Session.ForcedReadManifest", ManifestManager.ForcedReadManifest));
						list.Add(new KeyValuePair<string, object>("VS.TelemetryApi.DefaultChannels", StringExtensions.Join((IEnumerable<string>)defaultSessionChannelsId, ",")));
						diagnosticTelemetry.PostDiagnosticTelemetryWhenSessionInitialized(this, list);
						persistentPropertyBag.Clear();
						TelemetryEvent telemetryEvent;
						while (telemetryBufferChannel.TryDequeue(out telemetryEvent))
						{
							defaultContext.ProcessEvent(telemetryEvent);
							PostProcessedEvent(telemetryEvent);
						}
						isInitialized = true;
						while (telemetryBufferChannel.TryDequeue(out telemetryEvent))
						{
							defaultContext.ProcessEvent(telemetryEvent, false);
							PostProcessedEvent(telemetryEvent);
						}
						telemetryBufferChannel = null;
						identityTelemetry.IdentityInformationProvider.SchedulePostPersistedSharedPropertyAndSendAnyFaults(this, contextScheduler ?? new TelemetryScheduler());
						identityTelemetry.PostIdentityTelemetryWhenSessionInitialized(this);
					}
				}
			}
		}

		private bool ValidateHostName(string hostName)
		{
			if (string.IsNullOrEmpty(hostName))
			{
				return false;
			}
			if (hostName.Length < 1 || hostName.Length > 64)
			{
				return false;
			}
			return hostName.All((char c) => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '+' || c == ' ');
		}

		/// <summary>
		/// ToString to make debugging easier: show in debug watch window
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"'{SessionId}' Started = {isSessionStarted} OptIn={IsOptedIn} IsInitialized = {isInitialized} Cloned = {IsSessionCloned}";
		}
	}
}
