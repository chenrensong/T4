using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal static class TelemetryEventCorrelationCodeContract
	{
		internal static void RequireNotEmpty(this TelemetryEventCorrelation correlation, string argumentName)
		{
			if (correlation.IsEmpty)
			{
				throw new ArgumentException("Value shouldn't be empty.", argumentName);
			}
		}
	}
}
