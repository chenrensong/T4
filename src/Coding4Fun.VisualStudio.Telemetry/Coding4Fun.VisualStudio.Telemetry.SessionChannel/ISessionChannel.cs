using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Common interface for all session channels.
	/// Session channel depends on type of channel. It could be AppInsights channel, SQM channel, ETW and so on.
	/// Interface defines methods for sending events through the channel during one session.
	/// </summary>
	public interface ISessionChannel
	{
		/// <summary>
		/// Gets channel id
		/// </summary>
		string ChannelId
		{
			get;
		}

		/// <summary>
		/// Gets the transport used to post event.
		/// Format: id[.transport]
		/// Usually it just matches id, but sometime it more detailed.
		/// For example, in the case with Asimov channel it could be:
		/// aiutc.utc or aiutc.vortex
		/// </summary>
		string TransportUsed
		{
			get;
		}

		/// <summary>
		/// Gets or sets the type of a session
		/// </summary>
		ChannelProperties Properties
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether session already started
		/// </summary>
		/// <returns></returns>
		bool IsStarted
		{
			get;
		}

		/// <summary>
		/// Posts a telemetry event.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		void PostEvent(TelemetryEvent telemetryEvent);

		/// <summary>
		/// Posts a routed telemetry event.
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="args"></param>
		void PostEvent(TelemetryEvent telemetryEvent, IEnumerable<ITelemetryManifestRouteArgs> args);

		/// <summary>
		/// Start session channel. SessionId required for some channels.
		/// </summary>
		/// <param name="sessionId"></param>
		void Start(string sessionId);
	}
}
