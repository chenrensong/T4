using Coding4Fun.VisualStudio.Telemetry;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	internal class VSTelemetryScope<T> : TelemetryScopeBase where T : OperationEvent
	{
		private readonly TelemetryScope<T> telemetryScope;

		private bool disposed;

		public override TelemetryEventCorrelation Correlation => telemetryScope.Correlation;

		internal VSTelemetryScope(TelemetryScope<T> telemetryScope, TelemetryIdentifier identifier, ITelemetryRecorder telemetryRecorder)
			: base((OperationEvent)(object)telemetryScope.EndEvent, identifier, telemetryRecorder)
		{
			this.telemetryScope = telemetryScope;
		}

		internal VSTelemetryScope(TelemetryScope<T> telemetryScope, TelemetryIdentifier identifier, ITelemetryRecorder telemetryRecorder, ITelemetryScope parentScope)
			: base((OperationEvent)(object)telemetryScope.EndEvent, identifier, telemetryRecorder, parentScope)
		{
			this.telemetryScope = telemetryScope;
		}

		protected override void Dispose(bool disposing)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (!disposed && disposing)
			{
				base.Dispose(disposing);
				telemetryScope.End(base.Result, (string)null);
			}
			disposed = true;
		}
	}
}
