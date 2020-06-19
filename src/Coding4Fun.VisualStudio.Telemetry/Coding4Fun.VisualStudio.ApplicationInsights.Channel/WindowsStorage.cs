using System;
using System.Collections;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class WindowsStorage : PersistentStorageBase
	{
		private WindowsIdentity principal;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.WindowsStorage" /> class. This is the default Storage class for Windows
		/// </summary>
		/// <param name="uniqueFolderName">A folder name. Under this folder all the transmissions will be saved.</param>
		internal WindowsStorage(string uniqueFolderName)
			: base(uniqueFolderName)
		{
			principal = WindowsIdentity.GetCurrent();
		}

		protected override DirectoryInfo GetApplicationFolder()
		{
			IDictionary environmentVariables = Environment.GetEnvironmentVariables();
			var anon = new
			{
				RootPath = (environmentVariables["LOCALAPPDATA"] as string),
				AISubFolder = "Coding4Fun\\VSApplicationInsights"
			};
			var anon2 = new
			{
				RootPath = (environmentVariables["TEMP"] as string),
				AISubFolder = "Coding4Fun\\VSApplicationInsights"
			};
			var anon3 = new
			{
				RootPath = (environmentVariables["ProgramData"] as string),
				AISubFolder = "Coding4Fun\\VSApplicationInsights"
			};
			var array = new[]
			{
				anon,
				anon2,
				anon3
			};
			foreach (var anon4 in array)
			{
				try
				{
					if (!string.IsNullOrEmpty(anon4.RootPath))
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(anon4.RootPath);
						string path = Path.Combine(anon4.AISubFolder, FolderName);
						DirectoryInfo directoryInfo2 = directoryInfo.CreateSubdirectory(path);
						CheckAccessPermissions(directoryInfo2);
						return directoryInfo2;
					}
				}
				catch (UnauthorizedAccessException)
				{
				}
			}
			return null;
		}

		/// <summary>
		/// Simple method to detect if current user has permission to delete the file.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		protected override bool CanDelete(FileInfo fileInfo)
		{
			try
			{
				File.SetAttributes(fileInfo.FullName, FileAttributes.Normal);
			}
			catch
			{
				return false;
			}
			return true;
		}

		private static void CheckAccessPermissions(DirectoryInfo telemetryDirectory)
		{
			telemetryDirectory.GetFiles("_");
		}

		/// <summary>
		/// Convenience method to test if the right exists within the given rights
		/// </summary>
		/// <param name="right"></param>
		/// <param name="rule"></param>
		/// <returns></returns>
		private bool ContainsRights(FileSystemRights right, FileSystemAccessRule rule)
		{
			return (right & rule.FileSystemRights) == right;
		}
	}
}
