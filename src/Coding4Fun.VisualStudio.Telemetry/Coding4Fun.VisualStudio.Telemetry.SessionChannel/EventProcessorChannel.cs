using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	internal class EventProcessorChannel : TelemetryDisposableObject, ISessionChannel, IDisposeAndTransmit
	{
		private const int SchedulerDelay = 1;

		private readonly ConcurrentQueue<TelemetryEvent> queue = new ConcurrentQueue<TelemetryEvent>();

		private readonly IEventProcessor eventProcessor;

		private readonly ITelemetryScheduler scheduler;

		private readonly TelemetrySession telemetrySession;

		private bool hasProcessedEvents;

		private Action initializedAction = delegate
		{
		};

		/// <summary>
		/// Gets unique channel id
		/// </summary>
		public string ChannelId => "eventProcessorChannel";

		/// <summary>
		/// Gets or sets channel properties. It could restricts access to the channel.
		/// </summary>
		public ChannelProperties Properties
		{
			get
			{
				return ChannelProperties.None;
			}
			set
			{
				throw new MemberAccessException("it is not allowed to change properties for this channel");
			}
		}

		/// <summary>
		/// Gets a value indicating whether session is started
		/// </summary>
		/// <returns></returns>
		public bool IsStarted => true;

		public string TransportUsed => ChannelId;

		/// <summary>
		/// Sets the action that is fired after initial events have made it through to
		/// channels.
		/// </summary>
		internal Action InitializedAction
		{
			set
			{
				CodeContract.RequiresArgumentNotNull<Action>(value, "value");
				initializedAction = value;
			}
		}

		internal EventProcessorChannel(IEventProcessor theEventProcessor, ITelemetryScheduler theScheduler, TelemetrySession telemetrySession)
		{
			CodeContract.RequiresArgumentNotNull<IEventProcessor>(theEventProcessor, "theEventProcessor");
			CodeContract.RequiresArgumentNotNull<ITelemetryScheduler>(theScheduler, "theScheduler");
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			eventProcessor = theEventProcessor;
			scheduler = theScheduler;
			scheduler.InitializeTimed(TimeSpan.FromSeconds(1.0));
			this.telemetrySession = telemetrySession;
		}

		public void PostEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			if (!base.IsDisposed)
			{
				queue.Enqueue(telemetryEvent);
				Action action = ProcessEvents;
				scheduler.ScheduleTimed(action);
			}
		}

		/// <summary>
		/// Post routed event
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="args"></param>
		public void PostEvent(TelemetryEvent telemetryEvent, IEnumerable<ITelemetryManifestRouteArgs> args)
		{
			throw new ApplicationException("event is routed to the EventProcessor channel");
		}

		public void Start(string sessionId)
		{
		}

		/// <summary>
		/// Process events from the queue.
		/// Process all events.
		/// </summary>
		public void ProcessEvents()
		{
			try
			{
				if (base.IsDisposed)
				{
					throw new ObjectDisposedException(GetType().Name, "it is not allowed to process events after channel is disposed");
				}
				if (scheduler.CanEnterTimedDelegate())
				{
					TelemetryEvent result;
					while (queue.TryDequeue(out result))
					{
						eventProcessor.ProcessEvent(result);
					}
					if (!hasProcessedEvents)
					{
						hasProcessedEvents = true;
						initializedAction();
					}
				}
			}
			catch (Exception exceptionObject)
			{
				FaultEvent faultEvent = new FaultEvent("VS/Telemetry/InternalFault", $"Exception in SessionChannel.EventProcessorChannel ProcessEvents Channel = {ChannelId}", exceptionObject)
				{
					PostThisEventToTelemetry = false
				};
				faultEvent.AddProcessDump(Process.GetCurrentProcess().Id);
				telemetrySession.PostEvent(faultEvent);
			}
			finally
			{
				scheduler.ExitTimedDelegate();
			}
		}

		/// <summary>
		/// Transmit all internal buffers to the end-point and dispose channel.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns></returns>
		public async Task DisposeAndTransmitAsync(CancellationToken token)
		{
			DisposeInit();
			await eventProcessor.DisposeAndTransmitAsync(token).ConfigureAwait(false);
		}

		/// <summary>
		/// Implement Dispose resources
		/// </summary>
		protected override void DisposeManagedResources()
		{
			DisposeInit();
			eventProcessor.Dispose();
		}

		private void DisposeInit()
		{
			base.DisposeManagedResources();
			scheduler.CancelTimed(true);
			ProcessEvents();
		}

		public override string ToString()
		{
			return $"{ChannelId} QueueCnt = {queue.Count}";
		}
	}
}
