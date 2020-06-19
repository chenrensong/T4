namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Provides current optedin status for the user to experimentation service.
	/// </summary>
	public interface IExperimentationOptinStatusReader
	{
		/// <summary>
		/// Gets a value indicating whether user is optedin to the experimentation service.
		/// </summary>
		bool IsOptedIn
		{
			get;
		}
	}
}
