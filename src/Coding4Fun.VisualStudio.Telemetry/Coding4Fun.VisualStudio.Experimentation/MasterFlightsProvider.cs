using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal sealed class MasterFlightsProvider : TelemetryDisposableObject, IFlightsProvider, IDisposable
	{
		/// <summary>
		/// List of real flight providers
		/// </summary>
		private readonly IEnumerable<IFlightsProvider> inclusiveFlightsProviders;

		private readonly IEnumerable<IFlightsProvider> exclusiveFlightsProviders;

		private readonly IFlightsProvider shippedFlightsProvider;

		private readonly bool isUserOptedIn;

		private HashSet<string> activeFlights;

		public IEnumerable<string> Flights
		{
			get
			{
				RequiresNotDisposed();
				if (activeFlights == null)
				{
					activeFlights = BuildListOfFlights();
				}
				return activeFlights;
			}
		}

		public event EventHandler<FlightsEventArgs> FlightsUpdated;

		public MasterFlightsProvider(IEnumerable<IFlightsProvider> inclusiveFlightsProviders, IEnumerable<IFlightsProvider> exclusiveFlightsProviders, IFlightsProvider shippedFlightsProvider, IExperimentationOptinStatusReader optinStatusReader)
		{
			CodeContract.RequiresArgumentNotNull<IEnumerable<IFlightsProvider>>(inclusiveFlightsProviders, "inclusiveFlightsProviders");
			CodeContract.RequiresArgumentNotNull<IEnumerable<IFlightsProvider>>(exclusiveFlightsProviders, "exclusiveFlightsProviders");
			CodeContract.RequiresArgumentNotNull<IFlightsProvider>(shippedFlightsProvider, "shippedFlightsProvider");
			CodeContract.RequiresArgumentNotNull<IExperimentationOptinStatusReader>(optinStatusReader, "optinStatusReader");
			this.exclusiveFlightsProviders = exclusiveFlightsProviders;
			this.inclusiveFlightsProviders = inclusiveFlightsProviders;
			this.shippedFlightsProvider = shippedFlightsProvider;
			ForAllProviders(delegate(IFlightsProvider provider)
			{
				provider.FlightsUpdated += OnProviderFlightsUpdated;
			});
			isUserOptedIn = optinStatusReader.IsOptedIn;
		}

		public void Start()
		{
			RequiresNotDisposed();
			if (isUserOptedIn)
			{
				ForAllProviders(delegate(IFlightsProvider provider)
				{
					provider.Start();
				});
			}
		}

		public async Task WaitForReady(CancellationToken token)
		{
			RequiresNotDisposed();
			if (isUserOptedIn)
			{
				List<Task> tasks = new List<Task>();
				ForAllProviders(delegate(IFlightsProvider provider)
				{
					tasks.Add(provider.WaitForReady(token));
				});
				await Task.WhenAll(tasks).ConfigureAwait(false);
			}
		}

		protected override void DisposeManagedResources()
		{
			ForAllProviders(delegate(IFlightsProvider provider)
			{
				provider.Dispose();
			});
		}

		/// <summary>
		/// Perform action for all available providers
		/// </summary>
		/// <param name="action"></param>
		private void ForAllProviders(Action<IFlightsProvider> action)
		{
			ForAllProviders(action, inclusiveFlightsProviders.Union(exclusiveFlightsProviders));
		}

		private void ForAllProviders(Action<IFlightsProvider> action, IEnumerable<IFlightsProvider> flightsProviders)
		{
			foreach (IFlightsProvider flightsProvider in flightsProviders)
			{
				action(flightsProvider);
			}
		}

		private void OnProviderFlightsUpdated(object sender, FlightsEventArgs e)
		{
			HashSet<string> hashSet = BuildListOfFlights();
			if (!hashSet.SetEquals(activeFlights ?? new HashSet<string>()))
			{
				activeFlights = hashSet;
				OnFlightsUpdated();
			}
		}

		private HashSet<string> BuildListOfFlights()
		{
			HashSet<string> hashSet = new HashSet<string>();
			HashSet<string> enabledFlights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> disabledFlights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			ForAllProviders(delegate(IFlightsProvider provider)
			{
				enabledFlights.UnionWith(provider.Flights);
			}, inclusiveFlightsProviders);
			ForAllProviders(delegate(IFlightsProvider provider)
			{
				disabledFlights.UnionWith(provider.Flights);
			}, exclusiveFlightsProviders);
			if (disabledFlights.Contains("*") || !isUserOptedIn)
			{
				return new HashSet<string>(shippedFlightsProvider.Flights.Select((string flight) => flight.ToLowerInvariant()));
			}
			enabledFlights.ExceptWith(disabledFlights);
			return new HashSet<string>(enabledFlights.Select((string flight) => flight.ToLowerInvariant()));
		}

		private void OnFlightsUpdated()
		{
			this.FlightsUpdated?.Invoke(this, new FlightsEventArgs());
		}
	}
}
