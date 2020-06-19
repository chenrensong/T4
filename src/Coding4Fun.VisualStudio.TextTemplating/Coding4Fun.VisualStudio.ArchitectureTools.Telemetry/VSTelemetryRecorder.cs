using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
    internal sealed class VSTelemetryRecorder : ITelemetryRecorder, IDisposable
    {
        private bool disposed;

        private readonly TelemetrySession defaultSession;

        internal VSTelemetryRecorder()
        {
            //IL_0011: Unknown result type (might be due to invalid IL or missing references)
            //IL_001b: Expected O, but got Unknown
            defaultSession = (TelemetrySession)(object)new TelemetrySession(TelemetryService.DefaultSession.SerializeSettings());
            defaultSession.UseVsIsOptedIn();
            defaultSession.Start();
        }

        public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier)
        {
            return new VSTelemetryScope<UserTaskEvent>(TelemetrySessionExtensions.StartUserTask(defaultSession, telemetryIdentifier.Value), telemetryIdentifier, this);
        }

        public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, ITelemetryScope parentScope)
        {
            return new VSTelemetryScope<UserTaskEvent>(TelemetrySessionExtensions.StartUserTask(defaultSession, telemetryIdentifier.Value), telemetryIdentifier, this, parentScope);
        }

        public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties)
        {
            return new VSTelemetryScope<UserTaskEvent>(TelemetrySessionExtensions.StartUserTask(defaultSession, telemetryIdentifier.Value, (TelemetrySeverity)0, (IDictionary<string, object>)properties.ToDictionary((DataPoint dataPoint) => dataPoint.Identity.Value, (DataPoint dataPoint) => dataPoint.Value)), telemetryIdentifier, this);
        }

        public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties, ITelemetryScope parentScope)
        {
            return new VSTelemetryScope<UserTaskEvent>(TelemetrySessionExtensions.StartUserTask(defaultSession, telemetryIdentifier.Value, (TelemetrySeverity)0, (IDictionary<string, object>)properties.ToDictionary((DataPoint dataPoint) => dataPoint.Identity.Value, (DataPoint dataPoint) => dataPoint.Value)), telemetryIdentifier, this, parentScope);
        }

        public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier)
        {
            return new VSTelemetryScope<OperationEvent>(TelemetrySessionExtensions.StartOperation(defaultSession, telemetryIdentifier.Value), telemetryIdentifier, this);
        }

        public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, ITelemetryScope parentScope)
        {
            return new VSTelemetryScope<OperationEvent>(TelemetrySessionExtensions.StartOperation(defaultSession, telemetryIdentifier.Value), telemetryIdentifier, this, parentScope);
        }

        public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties)
        {
            return new VSTelemetryScope<OperationEvent>(TelemetrySessionExtensions.StartOperation(defaultSession, telemetryIdentifier.Value, (TelemetrySeverity)0, (IDictionary<string, object>)properties.ToDictionary((DataPoint dataPoint) => dataPoint.Identity.Value, (DataPoint dataPoint) => dataPoint.Value)), telemetryIdentifier, this);
        }

        public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties, ITelemetryScope parentScope)
        {
            return new VSTelemetryScope<OperationEvent>(TelemetrySessionExtensions.StartOperation(defaultSession, telemetryIdentifier.Value, (TelemetrySeverity)0, (IDictionary<string, object>)properties.ToDictionary((DataPoint dataPoint) => dataPoint.Identity.Value, (DataPoint dataPoint) => dataPoint.Value)), telemetryIdentifier, this, parentScope);
        }

        public void RecordEvent(TelemetryEvent telemetryEvent)
        {
            defaultSession.PostEvent(telemetryEvent);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && !((TelemetryDisposableObject)defaultSession).IsDisposed)
                {
                    try
                    {
                        ((TelemetryDisposableObject)defaultSession).Dispose();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                disposed = true;
            }
        }
    }
}
