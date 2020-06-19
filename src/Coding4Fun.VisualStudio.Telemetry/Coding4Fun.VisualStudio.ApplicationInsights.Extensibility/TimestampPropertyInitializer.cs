using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// An <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.ITelemetryInitializer" /> that sets <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry.Timestamp" /> to <see cref="P:System.DateTimeOffset.Now" />.
	/// </summary>
	public sealed class TimestampPropertyInitializer : ITelemetryInitializer
	{
		/// <summary>
		/// Sets <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry.Timestamp" /> to <see cref="P:System.DateTimeOffset.Now" />.
		/// </summary>
		public void Initialize(ITelemetry telemetry)
		{
			if (telemetry.Timestamp == default(DateTimeOffset))
			{
				telemetry.Timestamp = Clock.Instance.Time;
			}
		}
	}
}
