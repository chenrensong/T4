using System;
using System.IO;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Read OptedIn value for the VS for Mac.
	/// </summary>
	internal sealed class MacVsOptinStatusReader : ITelemetryOptinStatusReader
	{
		public bool ReadIsOptedInStatus(string productVersion)
		{
			try
			{
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				folderPath = Path.Combine(folderPath, "Library");
				folderPath = Path.Combine(folderPath, "Preferences");
				folderPath = Path.Combine(folderPath, "VisualStudio");
				if (Directory.Exists(folderPath))
				{
					string path = Path.Combine(folderPath, "TelemetryOptInState");
					if (File.Exists(path))
					{
						return File.ReadAllText(path) != "0";
					}
				}
			}
			catch
			{
			}
			return false;
		}

		/// <summary>
		/// For Mac we don't have separate OptinStatus for different versions.
		/// </summary>
		/// <returns></returns>
		public bool ReadIsOptedInStatus(TelemetrySession session)
		{
			return ReadIsOptedInStatus(string.Empty);
		}
	}
}
