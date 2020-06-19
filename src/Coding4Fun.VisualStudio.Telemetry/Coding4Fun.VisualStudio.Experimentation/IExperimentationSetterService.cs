namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// IExperimentationSetterService provides functionality to set particular flight with expiration date.
	/// </summary>
	public interface IExperimentationSetterService
	{
		/// <summary>
		/// Set flight for this machine using flightName as a flight and timeoutInMinutes as an expiration timeout.
		/// </summary>
		/// <param name="flightName"></param>
		/// <param name="timeoutInMinutes"></param>
		void SetFlight(string flightName, int timeoutInMinutes);
	}
}
