using System;
using System.IO;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class LinuxStorage : PersistentStorageBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.LinuxStorage" /> class.
		/// </summary>
		/// <param name="uniqueFolderName">A folder name. Under this folder all the transmissions will be saved.</param>
		internal LinuxStorage(string uniqueFolderName)
			: base(uniqueFolderName)
		{
		}

		protected override DirectoryInfo GetApplicationFolder()
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string text = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
			if (string.IsNullOrEmpty(text))
			{
				text = Path.Combine(folderPath, ".cache");
			}
			string path = Path.Combine("Coding4Fun", "ApplicationInsights", FolderName);
			return new DirectoryInfo(text).CreateSubdirectory(path);
		}

		/// <summary>
		/// Simple method to detect if current user has permission to delete the file.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns>true</returns>
		protected override bool CanDelete(FileInfo fileInfo)
		{
			return File.Exists(fileInfo.FullName);
		}
	}
}
