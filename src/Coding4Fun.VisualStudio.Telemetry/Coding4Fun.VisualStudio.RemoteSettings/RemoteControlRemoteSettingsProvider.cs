using Coding4Fun.VisualStudio.Telemetry.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class RemoteControlRemoteSettingsProvider : RemoteSettingsProviderBase, IRemoteSettingsProvider, ISettingsCollection, IDisposable
	{
		private const int DisposingIsStarted = 1;

		private const int DisposingNotStarted = 0;

		private const string RemoteSettingsTelemetryEventPath = "VS/Core/RemoteSettings/";

		private const string RemoteSettingsTelemetryPropertyPath = "VS.Core.RemoteSettings.";

		private readonly IVersionedRemoteSettingsStorageHandler remoteSettingsStorageHandler;

		private readonly IRemoteSettingsTelemetry remoteSettingsTelemetry;

		private readonly Lazy<IRemoteFileReader> remoteFileReader;

		private readonly IRemoteSettingsParser remoteSettingsParser;

		private readonly IScopeParserFactory scopeParserFactory;

		private readonly IRemoteSettingsValidator remoteSettingsValidator;

		private string fileName;

		private int startedDisposing;

		private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		public override string Name => "RemoteControl: " + fileName;

		public RemoteControlRemoteSettingsProvider(RemoteSettingsInitializer initializer)
			: base(initializer.VersionedRemoteSettingsStorageHandler, initializer.RemoteSettingsLogger)
		{
			remoteSettingsTelemetry = initializer.Telemetry;
			remoteSettingsStorageHandler = initializer.VersionedRemoteSettingsStorageHandler;
			fileName = initializer.RemoteSettingsFileName;
			remoteFileReader = new Lazy<IRemoteFileReader>(() => initializer.RemoteFileReaderFactory.Instance());
			remoteSettingsParser = initializer.RemoteSettingsParser;
			scopeParserFactory = initializer.ScopeParserFactory;
			remoteSettingsValidator = initializer.RemoteSettingsValidator;
		}

		/// <summary>
		/// Starts a background operation to check for new Remote Settings and apply them.
		/// </summary>
		/// <returns>Remote settings from file</returns>
		public override Task<GroupedRemoteSettings> Start()
		{
			RequiresNotDisposed();
			startTask = Task.Run(async delegate
			{
				string settingsFileEventName = "VS/Core/RemoteSettings/GetSettingsFileContent";
				Dictionary<string, object> settingsFileEventProperties = new Dictionary<string, object>();
				using (Stream stream = await remoteFileReader.Value.ReadFileAsync())
				{
					if (stream != null)
					{
						settingsFileEventProperties["VS.Core.RemoteSettings.GetContentsSucceeded"] = true;
						remoteSettingsTelemetry.PostEvent(settingsFileEventName, settingsFileEventProperties);
						string name = "VS/Core/RemoteSettings/ParseSettings";
						Dictionary<string, object> dictionary = new Dictionary<string, object>();
						VersionedDeserializedRemoteSettings versionedDeserializedRemoteSettings = remoteSettingsParser.TryParseVersionedStream(stream);
						if (!versionedDeserializedRemoteSettings.Successful)
						{
							logger.LogError("Error deserializing RemoteControl file: " + versionedDeserializedRemoteSettings.Error);
							dictionary["VS.Core.RemoteSettings.ErrorMessage"] = versionedDeserializedRemoteSettings.Error;
							remoteSettingsTelemetry.PostEvent(name, dictionary);
							ValidateStoredRemoteSettings();
							return null;
						}
						logger.LogVerbose("Got " + Name + " settings of version " + versionedDeserializedRemoteSettings.FileVersion + " with ChangesetId " + versionedDeserializedRemoteSettings.ChangesetId);
						dictionary["VS.Core.RemoteSettings.SettingsFileVersion"] = versionedDeserializedRemoteSettings.FileVersion;
						dictionary["VS.Core.RemoteSettings.SettingsFileChangeSet"] = versionedDeserializedRemoteSettings.ChangesetId;
						remoteSettingsTelemetry.PostEvent(name, dictionary);
						ProcessRemoteSettingsFile(versionedDeserializedRemoteSettings);
						return new GroupedRemoteSettings(versionedDeserializedRemoteSettings, Name);
					}
					if (startedDisposing == 0)
					{
						settingsFileEventProperties["VS.Core.RemoteSettings.GetContentsSucceeded"] = false;
						remoteSettingsTelemetry.PostEvent(settingsFileEventName, settingsFileEventProperties);
						ValidateStoredRemoteSettings();
					}
				}
				return null;
			});
			return startTask;
		}

		/// <inheritdoc />
		protected override void DisposeManagedResources()
		{
			if (Interlocked.CompareExchange(ref startedDisposing, 1, 0) != 1)
			{
				cancellationTokenSource.Cancel();
				if (remoteFileReader.IsValueCreated)
				{
					remoteFileReader.Value.Dispose();
				}
			}
		}

		private void ProcessRemoteSettingsFile(VersionedDeserializedRemoteSettings remoteSettings)
		{
			if (!cancellationTokenSource.IsCancellationRequested)
			{
				if (remoteSettingsStorageHandler.DoSettingsNeedToBeUpdated(remoteSettings.FileVersion))
				{
					IRemoteSettingsTelemetryActivity remoteSettingsTelemetryActivity = remoteSettingsTelemetry.CreateActivity("VS/Core/RemoteSettings/Apply");
					using (Mutex mutex = new Mutex(false, "Global\\7BCAEF5B-E7EA-428D-84AF-105BCD4D93FC-" + fileName.Replace('.', '-')))
					{
						bool flag = false;
						try
						{
							flag = mutex.WaitOne(-1, false);
						}
						catch (AbandonedMutexException)
						{
							flag = true;
						}
						if (flag)
						{
							if (!remoteSettingsStorageHandler.DoSettingsNeedToBeUpdated(remoteSettings.FileVersion))
							{
								return;
							}
							remoteSettingsTelemetryActivity.Start();
							logger.LogVerbose("Applying new settings for " + Name);
							if (remoteSettingsStorageHandler.FileVersion == string.Empty)
							{
								remoteSettingsStorageHandler.DeleteAllSettings();
								remoteSettingsStorageHandler.SaveSettings(remoteSettings);
							}
							else
							{
								remoteSettingsStorageHandler.DeleteSettingsForFileVersion(remoteSettings.FileVersion);
								remoteSettingsStorageHandler.SaveSettings(remoteSettings);
								remoteSettingsStorageHandler.CleanUpOldFileVersions(remoteSettings.FileVersion);
							}
							remoteSettingsTelemetryActivity.End();
							mutex.ReleaseMutex();
						}
					}
					remoteSettingsTelemetryActivity.Post(new Dictionary<string, object>
					{
						{
							"VS.Core.RemoteSettings.SettingsCount",
							remoteSettings.Settings.Count
						}
					});
				}
				else
				{
					logger.LogVerbose("Settings for " + Name + " are the same as cached version");
					ValidateStoredRemoteSettings();
				}
			}
		}

		private void ValidateStoredRemoteSettings()
		{
			if (!cancellationTokenSource.IsCancellationRequested)
			{
				try
				{
					remoteSettingsValidator.ValidateStored();
				}
				catch (RemoteSettingsValidationException exception)
				{
					logger.LogError("Stored remote settings not validated", exception);
					remoteSettingsStorageHandler.InvalidateFileVersion();
				}
			}
		}
	}
}
