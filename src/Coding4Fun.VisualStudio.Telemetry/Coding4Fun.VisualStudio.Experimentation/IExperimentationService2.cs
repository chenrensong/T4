using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// IExperimentationService2 provides information about all cached flights.
	/// </summary>
	public interface IExperimentationService2
	{
		/// <summary>
		/// Gets all enabled cached flights
		/// </summary>
		IEnumerable<string> AllEnabledCachedFlights
		{
			get;
		}
	}
}
