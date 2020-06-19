using Coding4Fun.VisualStudio.Experimentation;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class FlightScopeFilterProvider : IMultiValueScopeFilterAsyncProvider<BoolScopeValue>, IMultiValueScopeFilterProvider<BoolScopeValue>, IScopeFilterProvider
	{
		private readonly IExperimentationService experimentationService;

		public string Name => "Flight";

		public FlightScopeFilterProvider(IExperimentationService experimentationService)
		{
			CodeContract.RequiresArgumentNotNull<IExperimentationService>(experimentationService, "experimentationService");
			this.experimentationService = experimentationService;
		}

		/// <summary>
		/// Handles requests for Flight.NameOfFlight
		/// </summary>
		/// <param name="key">Name of the flight</param>
		/// <returns>A True <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.BoolScopeValue" /> if flight is enabled.</returns>
		public BoolScopeValue Provide(string key)
		{
			return new BoolScopeValue(experimentationService.IsCachedFlightEnabled(key));
		}

		/// <summary>
		/// Handles async requests for Flight.NameOfFlight
		/// </summary>
		/// <param name="key">Name of the flight</param>
		/// <returns>A True <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.BoolScopeValue" /> if flight is enabled.</returns>
		public async Task<BoolScopeValue> ProvideAsync(string key)
		{
			return new BoolScopeValue(await experimentationService.IsFlightEnabledAsync(key, CancellationToken.None));
		}
	}
}
