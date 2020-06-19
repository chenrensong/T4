using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public class VSTelemetryService : ITelemetryService, IDisposable
	{
		private bool disposed;

		private ITelemetryRecorder telemetryRecorder;

		public VSTelemetryService()
			: this(new VSTelemetryRecorder())
		{
		}

		internal VSTelemetryService(ITelemetryRecorder telemetryRecorder)
		{
			this.telemetryRecorder = (telemetryRecorder ?? throw new ArgumentNullException("telemetryRecorder"));
		}

		public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier)
		{
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			return telemetryRecorder.StartOperation(telemetryIdentifier);
		}

		public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties)
		{
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			return telemetryRecorder.StartOperation(telemetryIdentifier, properties);
		}

		public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier)
		{
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			return telemetryRecorder.StartUserTask(telemetryIdentifier);
		}

		public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties)
		{
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			return telemetryRecorder.StartUserTask(telemetryIdentifier, properties);
		}

		public void PostOperation(TelemetryIdentifier telemetryIdentifier, TelemetryResult result)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)new OperationEvent(telemetryIdentifier.Value, result, (string)null));
		}

		public void PostOperation(TelemetryIdentifier telemetryIdentifier, TelemetryResult result, IEnumerable<DataPoint> properties)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			OperationEvent val = (OperationEvent)(object)new OperationEvent(telemetryIdentifier.Value, result, (string)null);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostUserTask(TelemetryIdentifier telemetryIdentifier, TelemetryResult result)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)new UserTaskEvent(telemetryIdentifier.Value, result, (string)null));
		}

		public void PostUserTask(TelemetryIdentifier telemetryIdentifier, TelemetryResult result, IEnumerable<DataPoint> properties)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			UserTaskEvent val = (UserTaskEvent)(object)new UserTaskEvent(telemetryIdentifier.Value, result, (string)null);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description)
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			if (string.IsNullOrEmpty(description))
			{
				throw new ArgumentNullException("description");
			}
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, (Exception)null, (Func<IFaultUtility, int>)null));
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description, IEnumerable<DataPoint> properties)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			FaultEvent val = (FaultEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, (Exception)null, (Func<IFaultUtility, int>)null);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description, Exception exception)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, exception, (Func<IFaultUtility, int>)SendFaultToWatson));
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description, Exception exception, IEnumerable<DataPoint> properties)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			FaultEvent val = (FaultEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, exception, (Func<IFaultUtility, int>)SendFaultToWatson);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			telemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					telemetryRecorder.Dispose();
				}
				disposed = true;
			}
		}

		private static int SendFaultToWatson(IFaultUtility faultUtility)
		{
			return 0;
		}
	}
}
