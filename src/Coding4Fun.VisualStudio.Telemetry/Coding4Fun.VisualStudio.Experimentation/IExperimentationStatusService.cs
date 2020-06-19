using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// IExperimentationStatusService provides methods to query status of a flight without triggering the experimental scenario.
	/// </summary>
	public interface IExperimentationStatusService : IDisposable
	{
		/// <summary>
		/// Get status of the requested flight, if it is enabled for the user + filters. Fast and cheap method.
		/// Does not send a telemetry event to indicate a triggered experimental scenario.
		/// Read information from the local storage. Can be used on a startup.
		/// IsCachedFlightEnabled should be called at a later point when the experimental scenario will be triggered.
		/// </summary>
		/// <param name="flight">flight name is a string no more than 16 characters (case-insensitive)</param>
		/// <returns></returns>
		bool QueryCachedFlightStatus(string flight);

		/// <summary>
		/// Get actual flight status without sending a telemetry event to indicate a triggered experimental scenario.
		/// If requests in the progress waits on them.
		/// IsFlightEnabledAsync should be called at a later point when the experimental scenario will be triggered.
		/// </summary>
		/// <param name="flight">Interesting flight name (case-insensitive)</param>
		/// <param name="token">cancellation token to interrupt process</param>
		/// <returns></returns>
		Task<bool> QueryFlightStatusAsync(string flight, CancellationToken token);
	}
}
