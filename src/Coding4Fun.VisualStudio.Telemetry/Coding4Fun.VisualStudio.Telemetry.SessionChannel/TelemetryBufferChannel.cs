using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	internal class TelemetryBufferChannel : ISessionChannel
	{
		private readonly ConcurrentQueue<TelemetryEvent> eventBuffer = new ConcurrentQueue<TelemetryEvent>();

		public string ChannelId => "bufferChannel";

		public string TransportUsed => ChannelId;

		/// <summary>
		/// Gets or sets type of a session
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

		/// <summary>
		/// Post telemetry event information
		/// </summary>
		/// <param name="telemetryEvent"></param>
		public void PostEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			eventBuffer.Enqueue(telemetryEvent);
		}

		/// <summary>
		/// Post routed event
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="args"></param>
		public void PostEvent(TelemetryEvent telemetryEvent, IEnumerable<ITelemetryManifestRouteArgs> args)
		{
			PostEvent(telemetryEvent);
		}

		/// <summary>
		/// Session Start
		/// </summary>
		/// <param name="sessionID"></param>
		public void Start(string sessionID)
		{
		}

		/// <summary>
		/// Try to get current TelemetryEvent from the queue and remove it from queue
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <returns></returns>
		public bool TryDequeue(out TelemetryEvent telemetryEvent)
		{
			return eventBuffer.TryDequeue(out telemetryEvent);
		}

		public override string ToString()
		{
			return $"{ChannelId} Cnt = {eventBuffer.Count}";
		}
	}
}
