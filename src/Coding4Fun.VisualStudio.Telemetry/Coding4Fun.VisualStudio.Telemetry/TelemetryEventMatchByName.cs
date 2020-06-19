using Coding4Fun.VisualStudio.Utilities.Internal;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class is used to represent a telemetry filter by telemetry event name.
	/// </summary>
	public class TelemetryEventMatchByName : ITelemetryEventMatch
	{
		/// <summary>
		/// Gets the name of the event that you want to filter out in the notification service.
		/// </summary>
		public string EventName
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether the name of the event provided should match the telemetry event name exactly.
		/// </summary>
		public bool IsFullNameCheck
		{
			get;
			private set;
		}

		/// <summary>
		/// Create a filter for telemetry event by its name
		/// </summary>
		/// <param name="eventName">Name of the telemetry event you want to match</param>
		/// <param name="isFullNameCheck">Set to true to match the name exactly. Set to false for a startswith check</param>
		public TelemetryEventMatchByName(string eventName, bool isFullNameCheck)
		{
			CodeContract.RequiresArgumentNotNull<string>(eventName, "eventName");
			EventName = eventName;
			IsFullNameCheck = isFullNameCheck;
		}

		/// <inheritdoc />
		public bool IsEventMatch(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			if (IsFullNameCheck)
			{
				return telemetryEvent.Name.Equals(EventName, StringComparison.OrdinalIgnoreCase);
			}
			return telemetryEvent.Name.StartsWith(EventName, StringComparison.OrdinalIgnoreCase);
		}
	}
}
