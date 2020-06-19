using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.IO;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Asimov AppInsights channel. Post data to the Vortex directly or using UTC.
	/// </summary>
	internal sealed class AsimovAppInsightsSessionChannel : BaseAppInsightsSessionChannel
	{
		/// <summary>
		/// This path is for the folder with pending packets to send to backend.
		/// It should be in consistence with \Shared\AI\TelemetryChannels\PersistenceChannel\Windows\WindowsStorage.cs
		/// </summary>
		private const string AppInsightsPersistencePath = "Coding4Fun\\VSApplicationInsights";

		private readonly string channelId;

		private readonly bool isUtcEnabled;

		private readonly TelemetrySession hostTelemetrySession;

		private readonly IStorageBuilder storageBuilder;

		private readonly IProcessLockFactory processLockFactory;

		public override string ChannelId => channelId;

		public AsimovAppInsightsSessionChannel(string channelId, bool isUtcEnabled, string instrumentationKey, string userId, ChannelProperties channelProperties, TelemetrySession hostTelemetrySession, IStorageBuilder storageBuilder, IProcessLockFactory processLockFactory)
			: base(instrumentationKey, userId, null, channelProperties)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(channelId, "channelId");
			this.channelId = channelId;
			this.isUtcEnabled = isUtcEnabled;
			this.hostTelemetrySession = hostTelemetrySession;
			this.storageBuilder = storageBuilder;
			this.processLockFactory = processLockFactory;
		}

		/// <summary>
		/// Workaround for sending Asimov pending events which was sent by the VS Setup
		/// </summary>
		/// <param name="sessionId"></param>
		internal void CheckPendingEventsAndStartChannel(string sessionId)
		{
			string[] array = new string[3]
			{
				"LOCALAPPDATA",
				"TEMP",
				"ProgramData"
			};
			foreach (string environmentFolderName in array)
			{
				if (TryUploadPendingFiles(environmentFolderName, sessionId))
				{
					break;
				}
			}
		}

		/// <summary>
		/// Obtain AppInsights client wrapper
		/// </summary>
		/// <returns></returns>
		protected override IAppInsightsClientWrapper CreateAppInsightsClientWrapper()
		{
			return new AsimovAppInsightsClientWrapper(isUtcEnabled, InstrumentationKey, hostTelemetrySession, storageBuilder.Create(base.PersistenceFolderName), processLockFactory);
		}

		/// <summary>
		/// Try to check specified base path for the pending files and in case
		/// pending files are found explicitly start channel to send events.
		/// </summary>
		/// <param name="environmentFolderName"></param>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		private bool TryUploadPendingFiles(string environmentFolderName, string sessionId)
		{
			try
			{
				string environmentVariable = Environment.GetEnvironmentVariable(environmentFolderName);
				if (!string.IsNullOrEmpty(environmentVariable))
				{
					if (Directory.GetFiles(Path.Combine(Path.Combine(environmentVariable, "Coding4Fun\\VSApplicationInsights"), base.PersistenceFolderName), "*.trn", SearchOption.TopDirectoryOnly).Length != 0)
					{
						Start(sessionId);
					}
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}
	}
}
