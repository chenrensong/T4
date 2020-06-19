using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryLogSettingsProvider
	{
		/// <summary>
		/// Gets or sets the main identfiers of the file, could be a SessionId or ApplicationName/Version/BranchName
		/// </summary>
		IEnumerable<KeyValuePair<string, string>> MainIdentifiers
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the path of the file, usually C:\Something
		/// </summary>
		string Path
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the folder within Path to save files.
		/// </summary>
		string Folder
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the complete file path to actually create the file.
		/// </summary>
		string FilePath
		{
			get;
		}

		/// <summary>
		/// Get and, if needed, creates the folder where the log files will exist.
		/// </summary>
		/// <returns></returns>
		string GetCreateFolderPath();

		/// <summary>
		/// Gets the next (static) unique id for this log file. Used in case multiple files
		/// are created at the same second.
		/// </summary>
		/// <returns></returns>
		int GetNextUniqueId();
	}
}
