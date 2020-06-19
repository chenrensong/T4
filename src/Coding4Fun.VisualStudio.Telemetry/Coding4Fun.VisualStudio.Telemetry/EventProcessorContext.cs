using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Context is passed to the every single action during execution.
	/// Context contains all neccessary information for the action to execute.
	/// Context also contain event itself. During executing action event could be updated.
	/// </summary>
	internal sealed class EventProcessorContext : TelemetryDisposableObject, IEventProcessorContext, IDisposable
	{
		private readonly TelemetrySession hostTelemetrySession;

		private readonly IEventProcessorRouter eventProcessorRouter;

		private TelemetryEvent workerTelemetryEvent;

		private Dictionary<string, object> excludedProperties;

		/// <summary>
		/// Gets or sets a value indicating whether event is dropped
		/// This is analyzed at the very end of the processing
		/// </summary>
		public bool IsEventDropped
		{
			get;
			set;
		}

		public ThrottlingAction ThrottlingAction
		{
			get;
			set;
		}

		public TelemetrySession HostTelemetrySession => hostTelemetrySession;

		public IEventProcessorRouter Router => eventProcessorRouter;

		public TelemetryEvent TelemetryEvent => workerTelemetryEvent;

		public EventProcessorContext(TelemetrySession hostTelemetrySession, IEventProcessorRouter eventProcessorRouter)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(hostTelemetrySession, "hostTelemetrySession");
			CodeContract.RequiresArgumentNotNull<IEventProcessorRouter>(eventProcessorRouter, "eventProcessorRouter");
			this.hostTelemetrySession = hostTelemetrySession;
			this.eventProcessorRouter = eventProcessorRouter;
		}

		/// <summary>
		/// Reset ProcessorContext to be able to process new telemetryEvent
		/// and init all field.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		public void InitForNewEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			workerTelemetryEvent = telemetryEvent;
			IsEventDropped = false;
			excludedProperties = null;
			ThrottlingAction = ThrottlingAction.Default;
			Router.Reset();
		}

		/// <summary>
		/// Remove property from the property list of the event and move it to the
		/// excluded properties list
		/// </summary>
		/// <param name="propertyName"></param>
		public void ExcludePropertyFromEvent(string propertyName)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(propertyName, "propertyName");
			if (TelemetryEvent != null && TelemetryEvent.Properties.ContainsKey(propertyName))
			{
				if (excludedProperties == null)
				{
					excludedProperties = new Dictionary<string, object>();
				}
				excludedProperties[propertyName] = TelemetryEvent.Properties[propertyName];
				TelemetryEvent.Properties.Remove(propertyName);
			}
		}

		/// <summary>
		/// Move property from the excluded property list to the event properties list
		/// </summary>
		/// <param name="propertyName"></param>
		public void IncludePropertyToEvent(string propertyName)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(propertyName, "propertyName");
			if (excludedProperties != null && TelemetryEvent != null && excludedProperties.TryGetValue(propertyName, out object value))
			{
				TelemetryEvent.Properties[propertyName] = value;
			}
		}

		/// <summary>
		/// One more way to dispose router asynchronously.
		/// In that case it asks channels to flush everything to the Network asynchronously if channel supported such method.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Task for wait on</returns>
		public async Task DisposeAndTransmitAsync(CancellationToken token)
		{
			base.DisposeManagedResources();
			await Router.DisposeAndTransmitAsync(token).ConfigureAwait(false);
		}

		/// <summary>
		/// Dispose all channels
		/// </summary>
		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			Router.Dispose();
		}
	}
}
