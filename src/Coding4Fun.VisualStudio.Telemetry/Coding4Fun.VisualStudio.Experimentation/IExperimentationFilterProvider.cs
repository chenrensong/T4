namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// IExperimentationFilterProvider provides filter`s values.
	/// </summary>
	public interface IExperimentationFilterProvider
	{
		/// <summary>
		/// Get filter value by enum
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		string GetFilterValue(Filters filter);
	}
}
