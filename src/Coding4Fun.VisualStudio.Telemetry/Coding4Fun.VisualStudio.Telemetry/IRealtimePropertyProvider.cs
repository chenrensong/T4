using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IRealtimePropertyProvider : IPropertyProvider
	{
		void AddRealtimeSharedProperties(List<KeyValuePair<string, Func<object>>> sharedProperties, TelemetryContext telemetryContext);
	}
}
