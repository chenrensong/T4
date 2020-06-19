namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IEnvironmentTools
	{
		/// <summary>
		/// Gets current CLR version
		/// </summary>
		string Version
		{
			get;
		}

		/// <summary>
		/// Gets the string value of the environment variable
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		string GetEnvironmentVariable(string key);
	}
}
