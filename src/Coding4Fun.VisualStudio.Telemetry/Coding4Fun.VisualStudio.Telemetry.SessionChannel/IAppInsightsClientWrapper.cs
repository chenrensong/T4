using System;
using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
    internal interface IAppInsightsClientWrapper : IDisposable, IDisposeAndTransmit
	{
		string InstrumentationKey
		{
			get;
		}

		void Initialize(string sessionId, string userId);

		void TrackEvent(EventTelemetry ev);

		bool TryGetTransport(out string transportUsed);
	}
}
