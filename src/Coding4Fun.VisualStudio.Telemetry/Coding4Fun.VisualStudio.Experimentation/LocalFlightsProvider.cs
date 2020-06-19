using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal sealed class LocalFlightsProvider : TelemetryDisposableObject, IFlightsProvider, IDisposable
	{
		public static readonly string PathToSettingsPrefix = "Software\\Coding4Fun\\VisualStudio\\ABExp\\";

		private readonly object lockObject = new object();

		private readonly IKeyValueStorage keyValueStorage;

		private readonly string pathToSettings;

		private readonly object lockFlights = new object();

		private IEnumerable<string> flights;

		public IEnumerable<string> Flights
		{
			get
			{
				if (flights == null)
				{
					lock (lockFlights)
					{
						if (flights == null)
						{
							flights = ReadFlightsOnce();
						}
					}
				}
				return flights;
			}
			set
			{
				lock (lockFlights)
				{
					keyValueStorage.SetValue(pathToSettings, value.ToArray());
					flights = value;
				}
			}
		}

		public event EventHandler<FlightsEventArgs> FlightsUpdated
		{
			add
			{
			}
			remove
			{
			}
		}

		public LocalFlightsProvider(IKeyValueStorage keyValueStorage, string flightsKey)
		{
			CodeContract.RequiresArgumentNotNull<IKeyValueStorage>(keyValueStorage, "keyValueStorage");
			CodeContract.RequiresArgumentNotNullAndNotEmpty(flightsKey, "flightsKey");
			this.keyValueStorage = keyValueStorage;
			pathToSettings = PathToSettingsPrefix + flightsKey;
		}

		private IEnumerable<string> ReadFlightsOnce()
		{
			return keyValueStorage.GetValue(pathToSettings, new string[0]);
		}

		public void Start()
		{
		}

		public async Task WaitForReady(CancellationToken token)
		{
			await Task.FromResult<object>(null);
		}
	}
}
