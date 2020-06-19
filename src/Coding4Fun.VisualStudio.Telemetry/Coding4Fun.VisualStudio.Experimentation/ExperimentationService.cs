using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Experimentation service to provide functionality of A/B experiments:
	/// - reading flights;
	/// - caching current set of flights;
	/// - get answer on if flights are enabled.
	/// </summary>
	public sealed class ExperimentationService : TelemetryDisposableObject, IExperimentationService, IDisposable, IExperimentationSetterService, IExperimentationStatusService, IExperimentationService2
	{
		private sealed class FlightStatus
		{
			public bool IsEnabled
			{
				get;
				set;
			}

			public bool WasTriggered
			{
				get;
				set;
			}
		}

		private static readonly Lazy<ExperimentationService> defaultExperimentationService = new Lazy<ExperimentationService>(() => new ExperimentationService(ExperimentationServiceInitializer.BuildDefault()));

		private readonly IExperimentationTelemetry telemetry;

		private readonly IExperimentationFilterProvider filterProvider;

		private readonly ConcurrentDictionary<string, FlightStatus> flightStatus = new ConcurrentDictionary<string, FlightStatus>();

		private readonly IFlightsProvider flightsProvider;

		private readonly SetFlightsProvider setFlightsProvider;

		/// <summary>
		/// Gets default experimentation service
		/// </summary>
		[ExcludeFromCodeCoverage]
		public static IExperimentationService Default => defaultExperimentationService.Value;

		/// <summary>
		/// Gets default setter experimentation service
		/// </summary>
		[ExcludeFromCodeCoverage]
		public static IExperimentationSetterService DefaultSetter => defaultExperimentationService.Value;

		/// <summary>
		/// Gets default status experimentation service
		/// </summary>
		[ExcludeFromCodeCoverage]
		public static IExperimentationStatusService DefaultStatus => defaultExperimentationService.Value;

		/// <summary>
		/// Gets list of the enabled cached flights.
		/// </summary>
		/// <returns>List of enabled cached flights</returns>
		public IEnumerable<string> AllEnabledCachedFlights => flightsProvider.Flights;

		/// <summary>
		/// Construct experimentation service object using initializer object.
		/// </summary>
		/// <param name="initializer"></param>
		public ExperimentationService(ExperimentationServiceInitializer initializer)
		{
			CodeContract.RequiresArgumentNotNull<ExperimentationServiceInitializer>(initializer, "initializer");
			initializer.FillWithDefaults();
			telemetry = initializer.ExperimentationTelemetry;
			filterProvider = initializer.ExperimentationFilterProvider;
			flightsProvider = initializer.FlightsProvider;
			setFlightsProvider = initializer.SetFlightsProvider;
			flightsProvider.FlightsUpdated += OnFlightsUpdated;
			SetFlightsTelemetry();
		}

		/// <summary>
		/// Get status of the requested flight, if it is enabled for the user + filters. Fast and cheap method.
		/// Does not send a telemetry event to indicate a triggered experimental scenario.
		/// Read information from the local storage. Can be used on a startup.
		/// IsCachedFlightEnabled should be called at a later point when the experimental scenario will be triggered.
		/// </summary>
		/// <param name="flight">flight name is a string no more than 16 characters (case-insensitive)</param>
		/// <returns></returns>
		public bool QueryCachedFlightStatus(string flight)
		{
			return IsCachedFlightEnabledInternal(flight, false);
		}

		/// <summary>
		/// Get status of the requested flight, if it is enabled for the user + filters. Fast and cheap method.
		/// Read information from the local storage. Can be used on a startup.
		/// Sends telemetry event to indicate the triggered experimental scenario.
		/// </summary>
		/// <param name="flight">flight name is a string no more than 16 characters (case-insensitive)</param>
		/// <returns></returns>
		public bool IsCachedFlightEnabled(string flight)
		{
			return IsCachedFlightEnabledInternal(flight, true);
		}

		/// <summary>
		/// Get actual flight status without sending a telemetry event to indicate a triggered experimental scenario.
		/// If requests in the progress waits on them.
		/// IsFlightEnabledAsync should be called at a later point when the experimental scenario will be triggered.
		/// </summary>
		/// <param name="flight">Flight name (case-insensitive)</param>
		/// <param name="token">cancellation token to interrupt process</param>
		/// <returns></returns>
		public Task<bool> QueryFlightStatusAsync(string flight, CancellationToken token)
		{
			return IsFlightEnabledInternalAsync(flight, token, false);
		}

		/// <summary>
		/// Get actual flight status. If requests in the progress waits on them.
		/// Sends telemetry event to indicate the triggered experimental scenario.
		/// </summary>
		/// <param name="flight">Flight name (case-insensitive)</param>
		/// <param name="token">cancellation token to interrupt process</param>
		/// <returns></returns>
		public Task<bool> IsFlightEnabledAsync(string flight, CancellationToken token)
		{
			return IsFlightEnabledInternalAsync(flight, token, true);
		}

		/// <summary>
		/// Start the service. Ask all flights providers to start polling there endpoints to get actual flight set.
		/// </summary>
		public void Start()
		{
			RequiresNotDisposed();
			flightsProvider.Start();
		}

		/// <summary>
		/// Set flight for this machine using flightName as a flight and timeoutInMinutes as an expiration timeout.
		/// </summary>
		/// <param name="flightName"></param>
		/// <param name="timeoutInMinutes"></param>
		public void SetFlight(string flightName, int timeoutInMinutes)
		{
			RequiresNotDisposed();
			setFlightsProvider.SetFlight(flightName, timeoutInMinutes);
		}

		/// <summary>
		/// End of work with experimentation service. Release all resources.
		/// </summary>
		protected override void DisposeManagedResources()
		{
			flightsProvider.FlightsUpdated -= OnFlightsUpdated;
			flightsProvider.Dispose();
		}

		private void OnFlightsUpdated(object sender, FlightsEventArgs e)
		{
			SetFlightsTelemetry();
		}

		private void SetFlightsTelemetry()
		{
			telemetry.SetSharedProperty("VS.ABExp.Flights", string.Join(";", flightsProvider.Flights));
		}

		private void PostFlightRequestTelemetry(string flight, bool isEnabled)
		{
			telemetry.PostEvent("VS/ABExp/FlightRequest", new Dictionary<string, string>
			{
				{
					"VS.ABExp.Flight",
					flight
				},
				{
					"VS.ABExp.Result",
					isEnabled.ToString()
				}
			});
		}

		private bool IsCachedFlightEnabledInternal(string flight, bool sendTriggeredEvent)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(flight, "flight");
			RequiresNotDisposed();
			flight = flight.ToLowerInvariant();
			return flightStatus.AddOrUpdate(flight, delegate(string key)
			{
				bool isEnabled = flightsProvider.Flights.Contains(key);
				if (sendTriggeredEvent)
				{
					PostFlightRequestTelemetry(flight, isEnabled);
				}
				return new FlightStatus
				{
					IsEnabled = isEnabled,
					WasTriggered = sendTriggeredEvent
				};
			}, delegate(string key, FlightStatus existingValue)
			{
				if (sendTriggeredEvent && !existingValue.WasTriggered)
				{
					PostFlightRequestTelemetry(flight, existingValue.IsEnabled);
					existingValue.WasTriggered = true;
				}
				return existingValue;
			}).IsEnabled;
		}

		private async Task<bool> IsFlightEnabledInternalAsync(string flight, CancellationToken token, bool sendTriggeredEvent)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(flight, "flight");
			RequiresNotDisposed();
			await flightsProvider.WaitForReady(token).ConfigureAwait(false);
			return IsCachedFlightEnabledInternal(flight, sendTriggeredEvent);
		}
	}
}
