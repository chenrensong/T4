using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal abstract class BaseJsonLogFile<T> : TelemetryDisposableObject, ITelemetryLogFile<T>
	{
		private readonly object telemetryWriterLocker = new object();

		private ITelemetryWriter telemetryWriter;

		private bool writeComma;

		private bool isInitialized;

		protected ITelemetryLogSettingsProvider settingsProvider;

		public BaseJsonLogFile(ITelemetryWriter writer = null)
		{
			telemetryWriter = writer;
		}

		/// <summary>
		/// Ensure to create a new writer for each session
		/// </summary>
		/// <param name="settingsProvider"></param>
		public void Initialize(ITelemetryLogSettingsProvider settingsProvider)
		{
			if (!isInitialized)
			{
				lock (telemetryWriterLocker)
				{
					if (!isInitialized)
					{
						CodeContract.RequiresArgumentNotNull<ITelemetryLogSettingsProvider>(settingsProvider, "settingsProvider");
						this.settingsProvider = settingsProvider;
						if (telemetryWriter == null)
						{
							telemetryWriter = new TelemetryTextWriter(this.settingsProvider.FilePath);
						}
						WriteHeader();
						isInitialized = true;
					}
				}
			}
		}

		/// <summary>
		/// Log event to file
		/// </summary>
		/// <param name="eventData"></param>
		public void WriteAsync(T eventData)
		{
			if (!isInitialized)
			{
				throw new InvalidOperationException("JsonLogFile is not initialized");
			}
			if (telemetryWriter != null)
			{
				lock (telemetryWriterLocker)
				{
					if (telemetryWriter != null)
					{
						if (writeComma)
						{
							telemetryWriter.WriteLineAsync(",");
						}
						writeComma = true;
						telemetryWriter.WriteLineAsync(ConvertEventToString(eventData));
					}
				}
			}
		}

		protected abstract string ConvertEventToString(T eventData);

		/// <summary>
		/// Dispose managed resources.
		/// </summary>
		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			if (telemetryWriter != null)
			{
				lock (telemetryWriterLocker)
				{
					if (telemetryWriter != null)
					{
						WriteFooter();
						telemetryWriter.Dispose();
						telemetryWriter = null;
					}
				}
			}
		}

		private void WriteHeader()
		{
			telemetryWriter.WriteLineAsync("{");
			foreach (KeyValuePair<string, string> mainIdentifier in settingsProvider.MainIdentifiers)
			{
				telemetryWriter.WriteLineAsync(string.Format(CultureInfo.InvariantCulture, "\"{0}\":\"{1}\",", new object[2]
				{
					mainIdentifier.Key,
					mainIdentifier.Value
				}));
			}
			telemetryWriter.WriteLineAsync("\"events\":[");
		}

		private void WriteFooter()
		{
			telemetryWriter.WriteLineAsync("]");
			telemetryWriter.WriteLineAsync("}");
		}
	}
}
