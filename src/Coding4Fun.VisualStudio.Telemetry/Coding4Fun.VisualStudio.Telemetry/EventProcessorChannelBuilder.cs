using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// EventProcessorChannelBuilder serves to build a complex objects and takes care about all dependencies.
	/// There is 2 stages of the creating of all objects. 1 stage is to create Builder itself with all necessary
	/// parameters such as persistentPropertyBag. Second stage is to create objects itself when TelemetrySession
	/// object is known.
	/// </summary>
	internal sealed class EventProcessorChannelBuilder
	{
		private readonly IPersistentPropertyBag persistentPropertyBag;

		private readonly ITelemetryScheduler telemetryScheduler;

		public EventProcessor EventProcessor
		{
			get;
			private set;
		}

		public EventProcessorChannel EventProcessorChannel
		{
			get;
			private set;
		}

		public EventProcessorContext EventProcessorContext
		{
			get;
			private set;
		}

		public EventProcessorRouter EventProcessorRouter
		{
			get;
			private set;
		}

		/// <summary>
		/// Instantiate builder itself with all necessary parameters. It is done from the
		/// TelemetrySessionInitializer object, thus TelemetrySession doesn't care what external
		/// dependencies are needed for creating EventProcessorChannel and EventProcessor objects.
		/// </summary>
		/// <param name="persistentPropertyBag"></param>
		/// <param name="telemetryScheduler"></param>
		public EventProcessorChannelBuilder(IPersistentPropertyBag persistentPropertyBag, ITelemetryScheduler telemetryScheduler)
		{
			CodeContract.RequiresArgumentNotNull<IPersistentPropertyBag>(persistentPropertyBag, "persistentPropertyBag");
			CodeContract.RequiresArgumentNotNull<ITelemetryScheduler>(telemetryScheduler, "telemetryScheduler");
			this.persistentPropertyBag = persistentPropertyBag;
			this.telemetryScheduler = telemetryScheduler;
		}

		/// <summary>
		/// Build EventProcessorChannel and all its dependencies
		/// </summary>
		/// <param name="hostSession"></param>
		public void Build(TelemetrySession hostSession)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(hostSession, "hostSession");
			EventProcessorRouter = BuildRouter();
			EventProcessorContext = BuildContext(hostSession, EventProcessorRouter);
			EventProcessor = BuildProcessor(hostSession, EventProcessorContext);
			EventProcessorChannel = BuildChannel(EventProcessor, hostSession);
		}

		private EventProcessorChannel BuildChannel(IEventProcessor eventProcessor, TelemetrySession telemetrySession)
		{
			return new EventProcessorChannel(eventProcessor, telemetryScheduler, telemetrySession);
		}

		private EventProcessor BuildProcessor(TelemetrySession hostSession, IEventProcessorContext context)
		{
			return new EventProcessor(hostSession, context);
		}

		private EventProcessorContext BuildContext(TelemetrySession hostSession, IEventProcessorRouter eventProcessorRouter)
		{
			return new EventProcessorContext(hostSession, eventProcessorRouter);
		}

		private EventProcessorRouter BuildRouter()
		{
			return new EventProcessorRouter(persistentPropertyBag);
		}
	}
}
