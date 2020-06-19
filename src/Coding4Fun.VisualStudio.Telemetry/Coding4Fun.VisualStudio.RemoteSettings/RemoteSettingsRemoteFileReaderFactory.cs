using Coding4Fun.VisualStudio.RemoteControl;
using Coding4Fun.VisualStudio.Telemetry.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Implementation of the IRemoteFileReaderFactory which create preset IRemoteFileReader
	/// which uses RemoteControl to download shipping flights from the Azure account.
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal sealed class RemoteSettingsRemoteFileReaderFactory : IRemoteFileReaderFactory
	{
		private static readonly TimeSpan DownloadInterval = TimeSpan.FromMinutes(60.0);

		private const string DefaultBaseUrl = "https://az700632.vo.msecnd.net/pub";

		private const string DefaultHostId = "RemoteSettings";

		private const string DefaultPath = "RemoteSettings.json";

		private string fileNameOverride;

		public RemoteSettingsRemoteFileReaderFactory(string fileNameOverride)
		{
			this.fileNameOverride = fileNameOverride;
		}

		public IRemoteFileReader Instance()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			return new RemoteSettingsRemoteFileReader((IRemoteControlClient)(object)new RemoteControlClient("RemoteSettings", "https://az700632.vo.msecnd.net/pub", fileNameOverride ?? "RemoteSettings.json", (int)DownloadInterval.TotalMinutes, 60, 1));
		}
	}
}
