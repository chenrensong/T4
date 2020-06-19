using Coding4Fun.VisualStudio.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class RemoteSettingsLogger : TelemetryDisposableObject, IRemoteSettingsLogger, IDisposable
	{
		internal enum LoggingLevel
		{
			Verbose,
			Info,
			Error
		}

		internal class RemoteSettingsLogMessage
		{
			public DateTime Time
			{
				get;
				set;
			}

			[JsonConverter(typeof(StringEnumConverter))]
			public LoggingLevel Level
			{
				get;
				set;
			}

			public string Message
			{
				get;
				set;
			}

			public object Data
			{
				get;
				set;
			}

			public RemoteSettingsLogMessage()
			{
				Time = DateTime.Now;
			}
		}

		private const string RemoteSettingsLogFolderName = "VSRemoteSettingsLog";

		private readonly ITelemetryLogFile<RemoteSettingsLogMessage> logFile;

		private readonly ITelemetryLogSettingsProvider settingsProvider;

		private readonly RemoteSettingsFilterProvider filterProvider;

		private readonly bool loggingEnabled;

		private readonly Lazy<ConcurrentQueue<RemoteSettingsLogMessage>> buffer = new Lazy<ConcurrentQueue<RemoteSettingsLogMessage>>(() => new ConcurrentQueue<RemoteSettingsLogMessage>());

		private bool isStarted;

		private object flushLock = new object();

		public bool LoggingEnabled => loggingEnabled;

		private ConcurrentQueue<RemoteSettingsLogMessage> Buffer => buffer.Value;

		public RemoteSettingsLogger(RemoteSettingsFilterProvider remoteSettingsFilterProvider, bool loggingEnabled)
			: this(remoteSettingsFilterProvider, loggingEnabled, new RemoteSettingsJsonLogFile(), new TelemetryLogSettingsProvider())
		{
		}

		public RemoteSettingsLogger(RemoteSettingsFilterProvider filterProvider, bool loggingEnabled, ITelemetryLogFile<RemoteSettingsLogMessage> logFile, ITelemetryLogSettingsProvider settingsProvider)
		{
			this.filterProvider = filterProvider;
			this.loggingEnabled = loggingEnabled;
			this.logFile = logFile;
			this.settingsProvider = settingsProvider;
		}

		public Task Start()
		{
			if (!loggingEnabled)
			{
				return Task.FromResult<object>(null);
			}
			return Task.Run(delegate
			{
				FlushBufferAndStart();
			});
		}

		public void LogError(string message)
		{
			if (loggingEnabled)
			{
				LogMessage(new RemoteSettingsLogMessage
				{
					Level = LoggingLevel.Error,
					Message = message
				});
			}
		}

		public void LogError(string description, Exception exception)
		{
			LogError(description + ": " + exception.Message);
		}

		public void LogInfo(string message)
		{
			if (loggingEnabled)
			{
				LogMessage(new RemoteSettingsLogMessage
				{
					Level = LoggingLevel.Info,
					Message = message
				});
			}
		}

		public void LogVerbose(string message)
		{
			if (loggingEnabled)
			{
				LogMessage(new RemoteSettingsLogMessage
				{
					Level = LoggingLevel.Verbose,
					Message = message
				});
			}
		}

		public void LogVerbose(string message, object data)
		{
			if (loggingEnabled)
			{
				LogMessage(new RemoteSettingsLogMessage
				{
					Level = LoggingLevel.Verbose,
					Message = message,
					Data = data
				});
			}
		}

		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			if (loggingEnabled)
			{
				if (!isStarted)
				{
					FlushBufferAndStart();
				}
				(logFile as IDisposable)?.Dispose();
			}
		}

		private void LogMessage(RemoteSettingsLogMessage message)
		{
			if (!isStarted)
			{
				Buffer.Enqueue(message);
			}
			else
			{
				LogMessageNoBuffer(message);
			}
		}

		private void LogMessageNoBuffer(RemoteSettingsLogMessage message)
		{
			logFile.WriteAsync(message);
		}

		private void FlushBufferAndStart()
		{
			RemoteSettingsLogMessage result;
			lock (flushLock)
			{
				if (isStarted)
				{
					return;
				}
				List<KeyValuePair<string, string>> mainIdentifiers = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("applicationName", filterProvider.GetApplicationName()),
					new KeyValuePair<string, string>("applicationVersion", filterProvider.GetApplicationVersion()),
					new KeyValuePair<string, string>("branchName", filterProvider.GetBranchBuildFrom())
				};
				settingsProvider.MainIdentifiers = mainIdentifiers;
				settingsProvider.Path = Path.GetTempPath();
				settingsProvider.Folder = "VSRemoteSettingsLog";
				logFile.Initialize(settingsProvider);
				while (Buffer.TryDequeue(out result))
				{
					LogMessageNoBuffer(result);
				}
				isStarted = true;
			}
			while (Buffer.TryDequeue(out result))
			{
				LogMessageNoBuffer(result);
			}
		}
	}
}
