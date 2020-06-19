namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class that defines the strings used for property Result.
	/// </summary>
	public static class TelemetryResultStrings
	{
		/// <summary>
		/// Get Telemetry Result string used in backend.
		/// PLEASE DO NOT change the logic. Enum name is part of contract between client and backend server.
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		internal static string GetString(TelemetryResult result)
		{
			return result.ToString();
		}
	}
}
