using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Experimentation service provides A/B experimentation functionality.
	/// </summary>
	public interface IExperimentationService : IDisposable
	{
		/// <summary>
		/// Get status of the requested flight, if it is enabled for the user + filters. Fast and cheap method.
		/// Read information from the local storage. Can be used on a startup.
		/// </summary>
		/// <param name="flight">flight name is a string no more than 16 characters (case-insensitive)</param>
		/// <returns></returns>
		bool IsCachedFlightEnabled(string flight);

		/// <summary>
		/// Get actual flight status. If requests in the progress waits on them.
		/// </summary>
		/// <param name="flight">Interesting flight name (case-insensitive)</param>
		/// <param name="token">cancellation token to interrupt process</param>
		/// <returns></returns>
		Task<bool> IsFlightEnabledAsync(string flight, CancellationToken token);

		/// <summary>
		/// Start the service. Ask all flights providers to start polling there endpoints to get actual flight set.
		/// </summary>
		void Start();
	}
}
