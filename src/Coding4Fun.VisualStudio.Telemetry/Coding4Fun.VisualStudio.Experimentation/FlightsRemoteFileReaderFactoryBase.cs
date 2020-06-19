using Coding4Fun.VisualStudio.RemoteControl;
using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Implementation of the IRemoteFileReaderFactory which create preset IRemoteFileReader
	/// which uses RemoteControl to download shipping flights from the Azure account.
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal class FlightsRemoteFileReaderFactoryBase : IRemoteFileReaderFactory
	{
		private const int DownloadIntervalInMin = 60;

		private const string DefaultBaseUrl = "https://az700632.vo.msecnd.net/pub";

		private const string DefaultHostId = "FlightsData";

		private readonly string configPath;

		public FlightsRemoteFileReaderFactoryBase(string configPath)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(configPath, "configPath");
			this.configPath = configPath;
		}

		public IRemoteFileReader Instance()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			return new FlightsRemoteFileReader((IRemoteControlClient)(object)new RemoteControlClient("FlightsData", "https://az700632.vo.msecnd.net/pub", configPath, 60, 60, 1));
		}
	}
}
