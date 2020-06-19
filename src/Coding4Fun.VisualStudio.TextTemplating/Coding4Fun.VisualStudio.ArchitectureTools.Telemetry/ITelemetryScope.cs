using Coding4Fun.VisualStudio.Telemetry;
using System;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public interface ITelemetryScope : ITelemetryService, IDisposable
	{
		TelemetryEventCorrelation Correlation
		{
			get;
		}

		ITelemetryEvent EndEvent
		{
			get;
		}

		TelemetryResult Result
		{
			get;
			set;
		}

		ITelemetryScope Parent
		{
			get;
		}

		ITelemetryScope Root
		{
			get;
		}
	}
}
