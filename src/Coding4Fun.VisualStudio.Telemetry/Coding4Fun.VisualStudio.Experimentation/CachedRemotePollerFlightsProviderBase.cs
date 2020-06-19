using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Base class for the following functionality:
	/// - poll remote flight source with specific interval;
	/// - get current flights;
	/// - set current flights;
	/// - cache new flights/send signals;
	/// - cancel requests on dispose;
	/// - start polling.
	/// </summary>
	internal abstract class CachedRemotePollerFlightsProviderBase<FlightStreamType> : TelemetryDisposableObject, IFlightsProvider, IDisposable where FlightStreamType : IFlightsData
	{
		private readonly object lockObject = new object();

		private readonly Lazy<LocalFlightsProvider> cachedFlightsProvider;

		private readonly IFlightsStreamParser flightsStreamParser;

		private readonly Timer timer;

		private readonly int timerInterval;

		private bool isStarted;

		private Task firstTaskRequest;

		private HashSet<string> flights;

		public IEnumerable<string> Flights
		{
			get
			{
				RequiresNotDisposed();
				if (flights == null)
				{
					lock (lockObject)
					{
						if (flights == null)
						{
							flights = new HashSet<string>(cachedFlightsProvider.Value.Flights);
						}
					}
				}
				return flights;
			}
			private set
			{
				CodeContract.RequiresArgumentNotNull<IEnumerable<string>>(value, "value");
				if (!base.IsDisposed)
				{
					bool flag = false;
					lock (lockObject)
					{
						HashSet<string> equals = new HashSet<string>(value);
						if (flights == null || !flights.SetEquals(equals))
						{
							flights = equals;
							cachedFlightsProvider.Value.Flights = equals;
							flag = true;
						}
					}
					if (flag)
					{
						OnFlightsUpdated();
					}
				}
			}
		}

		public event EventHandler<FlightsEventArgs> FlightsUpdated;

		public CachedRemotePollerFlightsProviderBase(IKeyValueStorage keyValueStorage, IFlightsStreamParser flightsStreamParser, int timerInterval)
		{
			CodeContract.RequiresArgumentNotNull<IKeyValueStorage>(keyValueStorage, "keyValueStorage");
			CodeContract.RequiresArgumentNotNull<IFlightsStreamParser>(flightsStreamParser, "flightsStreamParser");
			cachedFlightsProvider = new Lazy<LocalFlightsProvider>(() => new LocalFlightsProvider(keyValueStorage, BuildFlightsKey()));
			this.flightsStreamParser = flightsStreamParser;
			this.timerInterval = timerInterval;
			timer = new Timer(async delegate
			{
				await SendRemoteRequestAsync();
			});
		}

		public void Start()
		{
			RequiresNotDisposed();
			if (!isStarted)
			{
				firstTaskRequest = SendRemoteRequestAsync();
				isStarted = true;
			}
		}

		public async Task WaitForReady(CancellationToken token)
		{
			RequiresNotDisposed();
			if (firstTaskRequest == null)
			{
				throw new InvalidOperationException("WaitForReady can't be called before calling Start()");
			}
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			token.Register(delegate
			{
				tcs.TrySetCanceled();
			}, false);
			await Task.WhenAny(firstTaskRequest, tcs.Task).ConfigureAwait(false);
			token.ThrowIfCancellationRequested();
		}

		protected override void DisposeManagedResources()
		{
			timer.Dispose();
			InternalDispose();
		}

		protected abstract Task<Stream> SendRemoteRequestInternalAsync();

		/// <summary>
		/// Build cach path key.
		/// </summary>
		/// <returns></returns>
		protected abstract string BuildFlightsKey();

		protected virtual void InternalDispose()
		{
		}

		private async Task SendRemoteRequestAsync()
		{
			RequiresNotDisposed();
			Stream stream = await SendRemoteRequestInternalAsync().ConfigureAwait(false);
			if (stream != null)
			{
				FlightStreamType val = await flightsStreamParser.ParseStreamAsync<FlightStreamType>(stream).ConfigureAwait(false);
				if (val != null)
				{
					Flights = val.Flights.Select((string f) => f.ToLowerInvariant());
				}
			}
			if (!base.IsDisposed)
			{
				timer.Change(timerInterval, -1);
			}
		}

		private void OnFlightsUpdated()
		{
			this.FlightsUpdated?.Invoke(this, new FlightsEventArgs());
		}
	}
}
