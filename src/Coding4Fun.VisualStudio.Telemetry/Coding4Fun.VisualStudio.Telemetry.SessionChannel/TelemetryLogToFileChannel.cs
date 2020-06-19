using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.IO;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Represents logging for posting events in the VSTelemetryLibrary to local log file
	/// </summary>
	internal class TelemetryLogToFileChannel : TelemetryDisposableObject, ISessionChannel
	{
		private const string TelemetryLogFolderName = "VSTelemetryLog";

		private readonly ITelemetryLogFile<TelemetryEvent> logFile;

		private readonly ITelemetryLogSettingsProvider settingsProvider;

		private ChannelProperties channelProperties = ChannelProperties.NotForUnitTest;

		private bool isChannelStarted;

		public string ChannelId => "fileLogger";

		public string TransportUsed => ChannelId;

		/// <summary>
		/// Gets or sets channel properties. It could restricts access to the channel.
		/// </summary>
		public ChannelProperties Properties
		{
			get
			{
				return channelProperties;
			}
			set
			{
				channelProperties = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether channel is started
		/// </summary>
		/// <returns></returns>
		public bool IsStarted => isChannelStarted;

		public TelemetryLogToFileChannel()
			: this(new TelemetryLogSettingsProvider(), new TelemetryJsonLogFile())
		{
		}

		/// <summary>
		/// The channel for logging telemetry events to file
		/// </summary>
		/// <param name="settingsProvider"></param>
		/// <param name="logFile"></param>
		internal TelemetryLogToFileChannel(ITelemetryLogSettingsProvider settingsProvider, ITelemetryLogFile<TelemetryEvent> logFile)
		{
			CodeContract.RequiresArgumentNotNull<ITelemetryLogSettingsProvider>(settingsProvider, "settingsProvider");
			CodeContract.RequiresArgumentNotNull<ITelemetryLogFile<TelemetryEvent>>(logFile, "logFile");
			this.logFile = logFile;
			this.settingsProvider = settingsProvider;
		}

		/// <summary>
		/// Post telemetry event information to log file
		/// </summary>
		/// <param name="telemetryEvent"></param>
		public void PostEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			logFile.WriteAsync(telemetryEvent);
		}

		/// <summary>
		/// Post routed event
		/// </summary>
		/// <param name="telemetryEvent"></param>
		/// <param name="args"></param>
		public void PostEvent(TelemetryEvent telemetryEvent, IEnumerable<ITelemetryManifestRouteArgs> args)
		{
			PostEvent(telemetryEvent);
		}

		/// <summary>
		/// Channel Start
		/// Get the session ID information and start a log file writer
		/// </summary>
		/// <param name="sessionID"></param>
		public void Start(string sessionID)
		{
			settingsProvider.MainIdentifiers = new KeyValuePair<string, string>[1]
			{
				new KeyValuePair<string, string>("session_id", sessionID)
			};
			settingsProvider.Path = Path.GetTempPath();
			settingsProvider.Folder = "VSTelemetryLog";
			isChannelStarted = true;
			logFile.Initialize(settingsProvider);
		}

		/// <summary>
		/// Dispose managed resources.
		/// </summary>
		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			(logFile as IDisposable)?.Dispose();
		}
	}
}
