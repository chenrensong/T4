using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IPropertyProvider
	{
		void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext);

		void PostProperties(TelemetryContext telemetryContext, CancellationToken token);
	}
}
