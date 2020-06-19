using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class PersistentSharedPropertyProvider : IPropertyProvider
	{
		private IPersistentPropertyBag persistedSessionProperties;

		public PersistentSharedPropertyProvider(IPersistentPropertyBag persistentPropertyBag)
		{
			CodeContract.RequiresArgumentNotNull<IPersistentPropertyBag>(persistentPropertyBag, "persistentPropertyBag");
			persistedSessionProperties = persistentPropertyBag;
		}

		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			IEnumerable<KeyValuePair<string, object>> allProperties = persistedSessionProperties.GetAllProperties();
			sharedProperties.AddRange(allProperties);
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
		}
	}
}
