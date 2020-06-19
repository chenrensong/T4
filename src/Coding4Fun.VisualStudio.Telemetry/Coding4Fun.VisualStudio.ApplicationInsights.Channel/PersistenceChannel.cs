using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Represents a communication channel for sending telemetry to Application Insights via HTTPS.
	/// </summary>
	internal sealed class PersistenceChannel : ITelemetryChannel, IDisposable, ISupportConfiguration
	{
		internal readonly TelemetryBuffer TelemetryBuffer;

		internal PersistenceTransmitter Transmitter;

		private readonly FlushManager flushManager;

		private bool developerMode;

		private int disposeCount;

		private int telemetryBufferSize;

		private StorageBase storage;

		/// <summary>
		/// Gets the storage unique folder.
		/// </summary>
		public string StorageUniqueFolder => Transmitter.StorageUniqueFolder;

		/// <summary>
		/// Gets or sets a value indicating whether developer mode of telemetry transmission is enabled.
		/// When developer mode is True, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> sends telemetry to Application Insights immediately
		/// during the entire lifetime of the application. When developer mode is False, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" />
		/// respects production sending policies defined by other properties.
		/// </summary>
		public bool DeveloperMode
		{
			get
			{
				return developerMode;
			}
			set
			{
				if (value != developerMode)
				{
					if (value)
					{
						telemetryBufferSize = TelemetryBuffer.Capacity;
						TelemetryBuffer.Capacity = 1;
					}
					else
					{
						TelemetryBuffer.Capacity = telemetryBufferSize;
					}
					developerMode = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets an interval between each successful sending.
		/// </summary>
		/// <remarks>On error scenario this value is ignored and the interval will be defined using an exponential back-off algorithm.</remarks>
		public TimeSpan SendingInterval
		{
			get
			{
				return Transmitter.SendingInterval;
			}
			set
			{
				Transmitter.SendingInterval = value;
			}
		}

		/// <summary>
		/// Gets or sets the interval between each flush to disk.
		/// </summary>
		public TimeSpan FlushInterval
		{
			get
			{
				return flushManager.FlushDelay;
			}
			set
			{
				flushManager.FlushDelay = value;
			}
		}

		/// <summary>
		/// Gets or sets the HTTP address where the telemetry is sent.
		/// </summary>
		public string EndpointAddress
		{
			get
			{
				return flushManager.EndpointAddress.ToString();
			}
			set
			{
				string uriString = value ?? "https://dc.services.visualstudio.com/v2/track";
				flushManager.EndpointAddress = new Uri(uriString);
			}
		}

		/// <summary>
		/// Gets or sets the maximum number of telemetry items that will accumulate in a memory before
		/// <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> persists them to disk.
		/// </summary>
		public int MaxTelemetryBufferCapacity
		{
			get
			{
				return TelemetryBuffer.Capacity;
			}
			set
			{
				TelemetryBuffer.Capacity = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum amount of disk space, in bytes, that <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> will
		/// use for storage.
		/// </summary>
		public ulong MaxTransmissionStorageCapacity
		{
			get
			{
				return storage.CapacityInBytes;
			}
			set
			{
				storage.CapacityInBytes = value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum amount of files allowed in storage. When the limit is reached telemetries will be dropped.
		/// </summary>
		public uint MaxTransmissionStorageFilesCapacity
		{
			get
			{
				return storage.MaxFiles;
			}
			set
			{
				storage.MaxFiles = value;
			}
		}

		/// <summary>
		/// Gets or sets the amount of time, in seconds, after application is started when the
		/// <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> will send telemetry to ApplicationInsights. Once the specified
		/// amount of time runs out, telemetry will be stored on disk until the application is started again.
		/// </summary>
		[Obsolete("This value is now obsolete and will be removed in next release. Currently it does nothing.")]
		public double StopUploadAfterIntervalInSeconds
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum telemetry batching interval. Once the interval expires, <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" />
		/// persists the accumulated telemetry items.
		/// </summary>
		[Obsolete("This value is now obsolete and will be removed in next release, use FlushInterval instead.")]
		public double DataUploadIntervalInSeconds
		{
			get
			{
				return flushManager.FlushDelay.Seconds;
			}
			set
			{
				flushManager.FlushDelay = TimeSpan.FromSeconds(value);
			}
		}

		/// <summary>
		/// Gets or sets the maximum amount of memory, in bytes, that <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> will use
		/// to buffer transmissions before sending them to Application Insights.
		/// </summary>
		[Obsolete("This value is now obsolete and will be removed in next release. Currently it does nothing.")]
		public int MaxTransmissionBufferCapacity
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum number of telemetry transmissions that <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> will
		/// send to Application Insights at the same time.
		/// </summary>
		[Obsolete("This value is now obsolete and will be removed in next release, use the sendersCount parameter in the constructor instead.")]
		public int MaxTransmissionSenderCapacity
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> class that uses a WindowsStorage instance to store transmission files.
		/// </summary>
		/// <remarks>Left in for unit tests which are, at this time, only able to be run on Windows.</remarks>
		public PersistenceChannel()
			: this(new WindowsStorage(null), new WindowsProcessLockFactory())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel" /> class.
		/// </summary>
		/// <param name="storage">
		/// A persistent storage that will store all transmissions.
		/// Setting this value groups channels, even from different processes.
		/// If 2 (or more) channels has the same <c>storage that share the same underlying folder</c> only one channel will perform the sending even if the channel is in a different process/AppDomain/Thread.
		/// </param>
		/// <param name="processLockFactory">
		/// IProcessLockFactory that creates an IProcessLock to sync transmission between processes
		/// </param>
		/// <param name="sendersCount">
		/// Defines the number of senders. A sender is a long-running thread that sends telemetry batches in intervals defined by <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistenceChannel.SendingInterval" />.
		/// So the amount of senders also defined the maximum amount of http channels opened at the same time.
		/// By default we have 1 sending thread which should be more than enough for our purposes.
		/// </param>
		public PersistenceChannel(StorageBase storage, IProcessLockFactory processLockFactory, int sendersCount = 1)
		{
			TelemetryBuffer = new TelemetryBuffer();
			this.storage = storage;
			Transmitter = new PersistenceTransmitter(this.storage, sendersCount, processLockFactory);
			flushManager = new FlushManager(this.storage, TelemetryBuffer);
			EndpointAddress = "https://dc.services.visualstudio.com/v2/track";
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose()
		{
			if (Interlocked.Increment(ref disposeCount) == 1)
			{
				if (Transmitter != null)
				{
					Transmitter.Dispose();
				}
				if (flushManager != null)
				{
					flushManager.Dispose();
				}
			}
		}

		/// <summary>
		/// Sends an instance of ITelemetry through the channel.
		/// </summary>
		public void Send(ITelemetry item)
		{
			TelemetryBuffer.Enqueue(item);
		}

		/// <summary>
		/// Flushes the in-memory buffer to disk.
		/// </summary>
		public void Flush()
		{
			flushManager.Flush();
		}

		/// <summary>
		/// Flushes the in-memory buffer to disk and tries to transmit all pending telemetry.
		/// </summary>
		/// <param name="token">Cancellation token in the case when consumer abandon waiting</param>
		/// <returns>Task so consumer can wait on it</returns>
		public async Task FlushAndTransmitAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			Flush();
			token.ThrowIfCancellationRequested();
			await Transmitter.Flush(token).ConfigureAwait(false);
		}

		/// <summary>
		/// Initialize method is called after all configuration properties have been loaded from the configuration.
		/// </summary>
		public void Initialize(TelemetryConfiguration configuration)
		{
		}
	}
}
