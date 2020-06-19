using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class EnvironmentTools : IEnvironmentTools
	{
		/// <summary>
		/// Gets the current CLR version
		/// </summary>
		public string Version => Environment.Version.ToString();

		/// <summary>
		/// Gets the string value of the environment variable
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetEnvironmentVariable(string key)
		{
			return Environment.GetEnvironmentVariable(key);
		}
	}
}
