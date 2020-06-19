using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class MacHostPropertyProvider : BaseHostRealtimePropertyProvider
	{
		private readonly IHostInformationProvider hostInfoProvider;

		private readonly INsBundleInformationProvider nsBundleInformationProvider;

		private readonly Lazy<string> hostExeName;

		private readonly Lazy<Version> hostVersionInfo;

		public MacHostPropertyProvider(IHostInformationProvider theHostInfoProvider, INsBundleInformationProvider theNsBundleInformationProvider)
			: base(theHostInfoProvider)
		{
			CodeContract.RequiresArgumentNotNull<IHostInformationProvider>(theHostInfoProvider, "theHostInfoProvider");
			hostInfoProvider = theHostInfoProvider;
			CodeContract.RequiresArgumentNotNull<INsBundleInformationProvider>(theNsBundleInformationProvider, "theNsBundleInformationProvider");
			nsBundleInformationProvider = theNsBundleInformationProvider;
			hostExeName = new Lazy<string>(() => InitializeHostExeName(), false);
			hostVersionInfo = new Lazy<Version>(() => InitializeHostVersionInfo(), false);
		}

		public override void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
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

		private string InitializeHostExeName()
		{
			return nsBundleInformationProvider.GetName();
		}

		private Version InitializeHostVersionInfo()
		{
			string version = nsBundleInformationProvider.GetVersion();
			if (version != null)
			{
				return new Version(version);
			}
			return null;
		}
	}
}
