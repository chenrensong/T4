using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal abstract class BaseHostRealtimePropertyProvider : IRealtimePropertyProvider, IPropertyProvider
	{
		private IHostInformationProvider hostInformationProvider;

		public BaseHostRealtimePropertyProvider(IHostInformationProvider hostInformationProvider)
		{
			CodeContract.RequiresArgumentNotNull<IHostInformationProvider>(hostInformationProvider, "hostInformationProvider");
			this.hostInformationProvider = hostInformationProvider;
		}

		public void AddRealtimeSharedProperties(List<KeyValuePair<string, Func<object>>> sharedProperties, TelemetryContext telemetryContext)
		{
			sharedProperties.Add(new KeyValuePair<string, Func<object>>("VS.Core.IsDebuggerAttached", () => hostInformationProvider.IsDebuggerAttached ? ((object)true) : null));
		}

		public abstract void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext);

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
		}
	}
}
