using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.ArchitectureTools.Telemetry
{
	public abstract class TelemetryScopeBase : ITelemetryScope, ITelemetryService, IDisposable
	{
		private bool disposed;

		private TelemetryResult result;

		private readonly TelemetryIdentifier TelemetryEvent;

		private readonly ITelemetryEvent endEvent;

		private readonly ITelemetryRecorder TelemetryRecorder;

		private readonly ITelemetryScope parentScope;

		private readonly ITelemetryScope rootScope;

		public abstract TelemetryEventCorrelation Correlation
		{
			get;
		}

		public ITelemetryEvent EndEvent => endEvent;

		public TelemetryResult Result
		{
			get
			{
				return result;
			}
			set
			{
				result = value;
			}
		}

		public ITelemetryScope Parent => parentScope;

		public ITelemetryScope Root => rootScope;

		protected TelemetryScopeBase(OperationEvent operationEvent, TelemetryIdentifier telemetryIdentifier, ITelemetryRecorder telemetryRecorder)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			result = (TelemetryResult)0;
			TelemetryEvent = (telemetryIdentifier ?? throw new ArgumentNullException("telemetryIdentifier"));
			TelemetryRecorder = (telemetryRecorder ?? throw new ArgumentNullException("telemetryRecorder"));
			endEvent = new VSTelemetryEvent(operationEvent);
			rootScope = this;
		}

		protected TelemetryScopeBase(OperationEvent operationEvent, TelemetryIdentifier telemetryIdentifier, ITelemetryRecorder telemetryRecorder, ITelemetryScope parentScope)
			: this(operationEvent, telemetryIdentifier, telemetryRecorder)
		{
			this.parentScope = parentScope;
			rootScope = (this.parentScope.Root ?? this.parentScope);
		}

		public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			ITelemetryScope telemetryScope = TelemetryRecorder.StartUserTask(telemetryIdentifier, this);
			endEvent.Correlate(telemetryScope.Correlation);
			return telemetryScope;
		}

		public ITelemetryScope StartUserTask(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			ITelemetryScope telemetryScope = TelemetryRecorder.StartUserTask(telemetryIdentifier, properties, this);
			endEvent.Correlate(telemetryScope.Correlation);
			return telemetryScope;
		}

		public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			ITelemetryScope telemetryScope = TelemetryRecorder.StartOperation(telemetryIdentifier, this);
			endEvent.Correlate(telemetryScope.Correlation);
			return telemetryScope;
		}

		public ITelemetryScope StartOperation(TelemetryIdentifier telemetryIdentifier, IEnumerable<DataPoint> properties)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			ITelemetryScope telemetryScope = TelemetryRecorder.StartOperation(telemetryIdentifier, properties, this);
			endEvent.Correlate(telemetryScope.Correlation);
			return telemetryScope;
		}

		public void PostUserTask(TelemetryIdentifier telemetryIdentifier, TelemetryResult result)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			UserTaskEvent val = (UserTaskEvent)(object)new UserTaskEvent(telemetryIdentifier.Value, result, (string)null);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostUserTask(TelemetryIdentifier telemetryIdentifier, TelemetryResult result, IEnumerable<DataPoint> properties)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			UserTaskEvent val = (UserTaskEvent)(object)new UserTaskEvent(telemetryIdentifier.Value, result, (string)null);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostOperation(TelemetryIdentifier telemetryIdentifier, TelemetryResult result)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			OperationEvent val = (OperationEvent)(object)new OperationEvent(telemetryIdentifier.Value, result, (string)null);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostOperation(TelemetryIdentifier telemetryIdentifier, TelemetryResult result, IEnumerable<DataPoint> properties)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			OperationEvent val = (OperationEvent)(object)new OperationEvent(telemetryIdentifier.Value, result, (string)null);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Expected O, but got Unknown
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			if (string.IsNullOrEmpty(description))
			{
				throw new ArgumentNullException("description");
			}
			FaultEvent val = (FaultEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, (Exception)null, (Func<IFaultUtility, int>)null);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description, IEnumerable<DataPoint> properties)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			FaultEvent val = (FaultEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, (Exception)null, (Func<IFaultUtility, int>)null);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description, Exception exception)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			FaultEvent val = (FaultEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, exception, (Func<IFaultUtility, int>)SendFaultToWatson);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void PostFault(TelemetryIdentifier telemetryIdentifier, string description, Exception exception, IEnumerable<DataPoint> properties)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (telemetryIdentifier == null)
			{
				throw new ArgumentNullException("telemetryIdentifier");
			}
			FaultEvent val = (FaultEvent)(object)new FaultEvent(telemetryIdentifier.Value, description, exception, (Func<IFaultUtility, int>)SendFaultToWatson);
			DataPointCollection.AddCollectionToDictionary(properties, ((TelemetryEvent)val).Properties);
			EndEvent.Correlate(((TelemetryEvent)val).Correlation);
			TelemetryRecorder.RecordEvent((TelemetryEvent)(object)val);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed && disposing)
			{
				for (ITelemetryScope telemetryScope = this; telemetryScope != null; telemetryScope = telemetryScope.Parent)
				{
					endEvent.SetProperties(telemetryScope.EndEvent.SharedProperties);
				}
			}
			disposed = true;
		}

		private static IDictionary<string, object> MergeProperties(IDictionary<string, object> first, IDictionary<string, object> second)
		{
			if (first == null)
			{
				return second;
			}
			if (second == null)
			{
				return first;
			}
			return first.Union(second).ToDictionary((KeyValuePair<string, object> keyValuePair) => keyValuePair.Key, (KeyValuePair<string, object> keyValuePair) => keyValuePair.Value);
		}

		private static int SendFaultToWatson(IFaultUtility faultUtility)
		{
			return 0;
		}
	}
}
