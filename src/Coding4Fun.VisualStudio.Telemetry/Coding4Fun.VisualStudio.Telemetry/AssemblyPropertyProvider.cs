using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class AssemblyPropertyProvider : IPropertyProvider
	{
		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			try
			{
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(GetType().Assembly.Location);
				sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.TelemetryApi.ProductVersion", versionInfo.ProductVersion));
			}
			catch (FileNotFoundException)
			{
			}
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
		}
	}
}
