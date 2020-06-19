using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal sealed class SetFlightsProvider : TelemetryDisposableObject, IFlightsProvider, IDisposable
	{
		internal sealed class FlightInformation
		{
			private static readonly char[] SplitCharacter = new char[1]
			{
				'#'
			};

			public string Flight
			{
				get;
				private set;
			}

			public DateTimeOffset ExpirationTime
			{
				get;
				private set;
			}

			public FlightInformation(string flight, DateTimeOffset expiration)
			{
				Flight = flight.ToLowerInvariant();
				ExpirationTime = expiration;
			}

			public override string ToString()
			{
				return Flight + "#" + ExpirationTime.ToString("u", CultureInfo.InvariantCulture);
			}

			/// <summary>
			/// Parse raw flight to the structure. Raw value is in format: flight#2017-12-12 03:24:59Z
			/// </summary>
			/// <param name="rawValue"></param>
			/// <returns></returns>
			public static FlightInformation Parse(string rawValue)
			{
				string[] array = rawValue.Split(SplitCharacter);
				if (array.Count() != 2)
				{
					return null;
				}
				DateTimeOffset expiration;
				try
				{
					expiration = DateTimeOffset.Parse(array[1], CultureInfo.InvariantCulture);
				}
				catch
				{
					return null;
				}
				return new FlightInformation(array[0], expiration);
			}
		}

		private readonly Lazy<LocalFlightsProvider> cachedFlightsProvider;

		private readonly object lockObject = new object();

		private HashSet<string> flights;

		/// <summary>
		/// Gets active flights set by the setter API. We get all information from the local storage.
		/// Flight list immutable once it is requested until the end of the process.
		/// That means if flight was set in this instance and flight list was requested before this call
		/// the flight will not be active until the next instance.
		/// </summary>
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
							flights = new HashSet<string>(ConvertRawDataToPlainFlights(GetRawFlights()));
						}
					}
				}
				return flights;
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

		public SetFlightsProvider(IKeyValueStorage keyValueStorage, string flightsKey)
		{
			CodeContract.RequiresArgumentNotNull<IKeyValueStorage>(keyValueStorage, "keyValueStorage");
			CodeContract.RequiresArgumentNotNullAndNotEmpty(flightsKey, "flightsKey");
			cachedFlightsProvider = new Lazy<LocalFlightsProvider>(() => new LocalFlightsProvider(keyValueStorage, flightsKey));
		}

		public async Task WaitForReady(CancellationToken token)
		{
			await Task.FromResult<object>(null);
		}

		public void Start()
		{
			List<string> list = new List<string>();
			bool flag = false;
			lock (lockObject)
			{
				foreach (string rawFlight in GetRawFlights())
				{
					FlightInformation flightInformation = FlightInformation.Parse(rawFlight);
					if (flightInformation != null && flightInformation.ExpirationTime > DateTimeOffset.UtcNow)
					{
						list.Add(rawFlight);
					}
					else
					{
						flag = true;
					}
				}
				if (flag)
				{
					SetRawFlights(list);
				}
			}
		}

		/// <summary>
		/// Set new flight.
		/// </summary>
		/// <param name="flightName"></param>
		/// <param name="timeoutInMinutes"></param>
		public void SetFlight(string flightName, int timeoutInMinutes)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(flightName, "flightName");
			if (timeoutInMinutes < 0)
			{
				throw new ArgumentException("Flight expiration timeout can't be negative", "timeoutInMinutes");
			}
			flightName = flightName.ToLowerInvariant();
			DateTimeOffset expiration = DateTimeOffset.UtcNow.AddMinutes(timeoutInMinutes);
			lock (lockObject)
			{
				IEnumerable<string> rawFlights = GetRawFlights();
				foreach (string item in rawFlights)
				{
					FlightInformation flightInformation = FlightInformation.Parse(item);
					if (flightInformation != null && flightInformation.Flight == flightName)
					{
						return;
					}
				}
				SetRawFlights(rawFlights.Union(new string[1]
				{
					new FlightInformation(flightName, expiration).ToString()
				}));
			}
		}

		private IEnumerable<string> GetRawFlights()
		{
			return cachedFlightsProvider.Value.Flights;
		}

		private void SetRawFlights(IEnumerable<string> rawFlights)
		{
			cachedFlightsProvider.Value.Flights = rawFlights;
		}

		/// <summary>
		/// Raw flight data are in the format: "flight_name#yyyy-MM-dd HH:mm:ssZ".
		/// First part (before '#') is the flight name and second part (after '#') is the expiration date of the flight.
		/// Return clean set of the flight (without expired flights).
		/// </summary>
		/// <param name="rawFlights"></param>
		/// <returns></returns>
		private IEnumerable<string> ConvertRawDataToPlainFlights(IEnumerable<string> rawFlights)
		{
			foreach (string rawFlight in rawFlights)
			{
				FlightInformation flightInformation = FlightInformation.Parse(rawFlight);
				if (flightInformation != null && flightInformation.ExpirationTime > DateTimeOffset.UtcNow)
				{
					yield return flightInformation.Flight;
				}
			}
		}
	}
}
