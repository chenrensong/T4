using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryLogSettingsProvider : ITelemetryLogSettingsProvider
	{
		/// <summary>
		/// We need it to create unique file across current process.
		/// It is possible to create several files with the same name if
		/// it is one process which creates several cloned sessions at the same time.
		/// Example - VsHub
		/// </summary>
		private static int fileVersion;

		private static int processId = -1;

		private static int appDomainId = -1;

		public string FileNameFormatString => "{0:yyyyMMdd_HHmmss}_{1}_{2}_{3}_{4}.txt";

		public IEnumerable<KeyValuePair<string, string>> MainIdentifiers
		{
			get;
			set;
		}

		public int ProcessId
		{
			get
			{
				if (processId == -1)
				{
					processId = Process.GetCurrentProcess().Id;
				}
				return processId;
			}
		}

		public int AppDomainId
		{
			get
			{
				if (appDomainId == -1)
				{
					appDomainId = AppDomain.CurrentDomain.Id;
				}
				return appDomainId;
			}
		}

		public string Path
		{
			get;
			set;
		}

		public string Folder
		{
			get;
			set;
		}

		public string FilePath
		{
			get
			{
				string path = string.Format(CultureInfo.InvariantCulture, FileNameFormatString, DateTime.Now, string.Join("_", from x in MainIdentifiers
					select x.Value into x
					where !string.IsNullOrEmpty(x)
					select x), ProcessId, AppDomainId, GetNextUniqueId());
				return System.IO.Path.Combine(GetCreateFolderPath(), path);
			}
		}

		public string GetCreateFolderPath()
		{
			string text = System.IO.Path.Combine(Path, Folder);
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			return text;
		}

		public int GetNextUniqueId()
		{
			return fileVersion++;
		}
	}
}
