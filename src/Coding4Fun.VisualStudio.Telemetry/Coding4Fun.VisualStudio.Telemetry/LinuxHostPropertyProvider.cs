using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class LinuxHostPropertyProvider : IPropertyProvider
	{
		private readonly IHostInformationProvider hostInfoProvider;

		private readonly Lazy<string> hostExeName;

		private readonly Lazy<Version> hostVersionInfo;

		public LinuxHostPropertyProvider(IHostInformationProvider theHostInfoProvider)
		{
			CodeContract.RequiresArgumentNotNull<IHostInformationProvider>(theHostInfoProvider, "theHostInfoProvider");
			hostInfoProvider = theHostInfoProvider;
			hostExeName = new Lazy<string>(() => InitializeHostExeName(), false);
			hostVersionInfo = new Lazy<Version>(() => InitializeHostVersionInfo(), false);
		}

		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			if (hostExeName.Value != null)
			{
				sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.ExeName", hostExeName.Value.ToLowerInvariant()));
			}
			else
			{
				sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.ExeName", hostInfoProvider.ProcessName));
			}
			if (hostVersionInfo.Value != null)
			{
				Version value = hostVersionInfo.Value;
				sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.ExeVersion", value.ToString()));
				sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.BuildNumber", value.Build));
			}
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.ProcessId", hostInfoProvider.ProcessId));
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
		}

		private string InitializeHostExeName()
		{
			return null;
		}

		private Version InitializeHostVersionInfo()
		{
			return null;
		}
	}
}
