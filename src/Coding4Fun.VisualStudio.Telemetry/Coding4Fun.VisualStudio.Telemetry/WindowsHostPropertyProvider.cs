using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class WindowsHostPropertyProvider : BaseHostRealtimePropertyProvider
	{
		private readonly IHostInformationProvider hostInfoProvider;

		private readonly Lazy<FileVersionInfo> hostVersionInfo;

		public WindowsHostPropertyProvider(IHostInformationProvider theHostInfoProvider)
			: base(theHostInfoProvider)
		{
			CodeContract.RequiresArgumentNotNull<IHostInformationProvider>(theHostInfoProvider, "theHostInfoProvider");
			hostInfoProvider = theHostInfoProvider;
			hostVersionInfo = new Lazy<FileVersionInfo>(() => InitializeHostVersionInfo(), false);
		}

		public override void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.ExeName", hostInfoProvider.ProcessName));
			if (hostVersionInfo.Value != null)
			{
				FileVersionInfo value = hostVersionInfo.Value;
				sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.ExeVersion", string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", value.FileMajorPart, value.FileMinorPart, value.FileBuildPart, value.FilePrivatePart)));
				sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.BuildNumber", value.FileBuildPart));
			}
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.ProcessId", hostInfoProvider.ProcessId));
		}

		private FileVersionInfo InitializeHostVersionInfo()
		{
			string fullProcessExeName = NativeMethods.GetFullProcessExeName();
			if (!string.IsNullOrEmpty(fullProcessExeName))
			{
				try
				{
					return FileVersionInfo.GetVersionInfo(fullProcessExeName);
				}
				catch (FileNotFoundException)
				{
				}
			}
			return null;
		}
	}
}
