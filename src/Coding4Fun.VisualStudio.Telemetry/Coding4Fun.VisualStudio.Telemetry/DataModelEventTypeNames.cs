namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class that defines string values used in event property EventType
	/// </summary>
	internal static class DataModelEventTypeNames
	{
		/// <summary>
		/// Get event type name from event type enum.
		/// PLEASE DO NOT change the code logic. Enum name is part of contract between client and backend server.
		/// </summary>
		/// <param name="eventType"></param>
		/// <returns></returns>
		internal static string GetName(DataModelEventType eventType)
		{
			return eventType.ToString();
		}
	}
}
