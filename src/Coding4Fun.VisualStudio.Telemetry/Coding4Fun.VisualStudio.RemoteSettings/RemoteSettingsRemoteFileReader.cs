using Coding4Fun.VisualStudio.RemoteControl;
using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Implementation of the IRemoteFileReader interface to read remote file using preset remote control instance.
	/// </summary>
	internal sealed class RemoteSettingsRemoteFileReader : TelemetryDisposableObject, IRemoteFileReader, IDisposable
	{
		private readonly IRemoteControlClient remoteControlClient;

		public RemoteSettingsRemoteFileReader(IRemoteControlClient remoteControlClient)
		{
			CodeContract.RequiresArgumentNotNull<IRemoteControlClient>(remoteControlClient, "remoteControlClient");
			this.remoteControlClient = remoteControlClient;
		}

		public async Task<Stream> ReadFileAsync()
		{
			RequiresNotDisposed();
			return await remoteControlClient.ReadFileAsync((BehaviorOnStale)2).ConfigureAwait(false);
		}

		protected override void DisposeManagedResources()
		{
			((IDisposable)remoteControlClient).Dispose();
		}
	}
}
