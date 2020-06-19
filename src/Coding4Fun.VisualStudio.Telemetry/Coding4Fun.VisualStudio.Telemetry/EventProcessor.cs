using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class EventProcessor : TelemetryDisposableObject, IEventProcessor, IDisposable
	{
		private readonly IEventProcessorContext eventProcessorContext;

		private readonly TelemetrySession mainSession;

		private readonly List<IEventProcessorAction> customActionList = new List<IEventProcessorAction>();

		private readonly object updateManifestLock = new object();

		private TelemetryManifest currentManifest;

		private bool diagnosticNeedsToBePosted = true;

		/// <summary>
		/// Gets or sets current manifest used to processing events
		/// </summary>
		internal TelemetryManifest CurrentManifest
		{
			get
			{
				return currentManifest;
			}
			set
			{
				CodeContract.RequiresArgumentNotNull<TelemetryManifest>(value, "value");
				UpdateManifest(value);
			}
		}

		internal IEventProcessorContext EventProcessorContext => eventProcessorContext;

		/// <summary>
		/// Event processor constructor should be provided with session.
		/// This required for start channels.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="eventProcessorContext"></param>
		public EventProcessor(TelemetrySession session, IEventProcessorContext eventProcessorContext)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(session, "session");
			CodeContract.RequiresArgumentNotNull<IEventProcessorContext>(eventProcessorContext, "eventProcessorContext");
			mainSession = session;
			this.eventProcessorContext = eventProcessorContext;
		}

		/// <summary>
		/// Process event, using dynamic telemetry settings.
		/// Called from the background thread.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		public void ProcessEvent(TelemetryEvent telemetryEvent)
		{
			RequiresNotDisposed();
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			if (CurrentManifest == null)
			{
				throw new NullReferenceException("CurrentManifest");
			}
			IEnumerable<IEventProcessorAction> mergedCustomAndManifestActionsInOrder = GetMergedCustomAndManifestActionsInOrder(CurrentManifest.GetActionsForEvent(telemetryEvent));
			bool flag = true;
			eventProcessorContext.InitForNewEvent(telemetryEvent);
			foreach (IEventProcessorAction item in mergedCustomAndManifestActionsInOrder)
			{
				flag = !item.Execute(eventProcessorContext);
				if (flag)
				{
					break;
				}
			}
			eventProcessorContext.Router.RouteEvent(telemetryEvent, mainSession.SessionId, flag || eventProcessorContext.IsEventDropped);
		}

		/// <summary>
		/// Add session channel
		/// </summary>
		/// <param name="channel"></param>
		public void AddChannel(ISessionChannel channel)
		{
			if (!base.IsDisposed)
			{
				CodeContract.RequiresArgumentNotNull<ISessionChannel>(channel, "channel");
				eventProcessorContext.Router.AddChannel(channel);
			}
		}

		/// <summary>
		/// Add custom action
		/// </summary>
		/// <param name="eventProcessorAction"></param>
		public void AddCustomAction(IEventProcessorAction eventProcessorAction)
		{
			CodeContract.RequiresArgumentNotNull<IEventProcessorAction>(eventProcessorAction, "eventProcessorAction");
			customActionList.Add(eventProcessorAction);
		}

		public void PostDiagnosticInformationIfNeeded()
		{
			UpdateManifest(null);
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
			await eventProcessorContext.DisposeAndTransmitAsync(token).ConfigureAwait(false);
		}

		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			eventProcessorContext.Dispose();
		}

		private void UpdateManifest(TelemetryManifest manifestToBeUpdated)
		{
			lock (updateManifestLock)
			{
				if (diagnosticNeedsToBePosted)
				{
					PostDiagnosticInformation(manifestToBeUpdated);
				}
				if (manifestToBeUpdated != null)
				{
					currentManifest = manifestToBeUpdated;
					diagnosticNeedsToBePosted = true;
				}
				else
				{
					diagnosticNeedsToBePosted = false;
				}
			}
		}

		/// <summary>
		/// Post diagnostic information about telemetry manifest rules and actions.
		/// </summary>
		/// <param name="newManifest"></param>
		private void PostDiagnosticInformation(TelemetryManifest newManifest)
		{
			foreach (IEventProcessorAction customAction in customActionList)
			{
				(customAction as IEventProcessorActionDiagnostics)?.PostDiagnosticInformation(mainSession, newManifest);
			}
		}

		/// <summary>
		/// Get predefined custom actions
		/// </summary>
		/// <param name="manifestActions"></param>
		/// <returns></returns>
		private IEnumerable<IEventProcessorAction> GetMergedCustomAndManifestActionsInOrder(IEnumerable<ITelemetryManifestAction> manifestActions)
		{
			return from action in manifestActions.Concat(customActionList)
				orderby action.Priority
				select action;
		}
	}
}
