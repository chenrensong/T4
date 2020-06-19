using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// An <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.ITelemetryInitializer" /> that that populates <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry.Sequence" /> property for
	/// the Coding4Fun internal telemetry sent to the Vortex endpoint.
	/// </summary>
	public sealed class SequencePropertyInitializer : ITelemetryInitializer
	{
		private readonly string stablePrefix = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=') + ":";

		private long currentNumber;

		/// <summary>
		/// Populates <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry.Sequence" /> with unique ID and sequential number.
		/// </summary>
		public void Initialize(ITelemetry telemetry)
		{
			if (string.IsNullOrEmpty(telemetry.Sequence))
			{
				telemetry.Sequence = stablePrefix + Interlocked.Increment(ref currentNumber);
			}
		}
	}
}
