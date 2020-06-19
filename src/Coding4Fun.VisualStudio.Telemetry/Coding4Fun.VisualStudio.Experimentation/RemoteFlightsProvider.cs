using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal sealed class RemoteFlightsProvider<T> : CachedRemotePollerFlightsProviderBase<T> where T : IFlightsData
	{
		private const int DefaultPollingIntervalInSecs = 1800000;

		private readonly string flightsKey;

		private readonly Lazy<IRemoteFileReader> remoteFileReader;

		public RemoteFlightsProvider(IKeyValueStorage keyValueStorage, string flightsKey, IRemoteFileReaderFactory remoteFileFactory, IFlightsStreamParser flightsStreamParser)
			: base(keyValueStorage, flightsStreamParser, 1800000)
		{
			CodeContract.RequiresArgumentNotNull<IRemoteFileReaderFactory>(remoteFileFactory, "remoteFileFactory");
			CodeContract.RequiresArgumentNotNullAndNotEmpty(flightsKey, "flightsKey");
			remoteFileReader = new Lazy<IRemoteFileReader>(() => remoteFileFactory.Instance());
			this.flightsKey = flightsKey;
		}

		protected override void InternalDispose()
		{
			if (remoteFileReader.IsValueCreated)
			{
				remoteFileReader.Value.Dispose();
			}
		}

		protected override async Task<Stream> SendRemoteRequestInternalAsync()
		{
			return await remoteFileReader.Value.ReadFileAsync().ConfigureAwait(false);
		}

		protected override string BuildFlightsKey()
		{
			return flightsKey;
		}
	}
}
