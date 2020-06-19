using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Globalization;
using System.IO;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// An instance of this class represents reading from a particular file from disk.
	/// </summary>
	internal class FileReader : IFileReader
	{
		private string filePath;

		private string fileDirectory;

		private const int MaxCharFilePath = 248;

		public FileReader(string filePath)
		{
			filePath.RequiresArgumentNotNull("filePath");
			if (filePath.Length > 248)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "File path can be a maximum of {0} characters", new object[1]
				{
					248
				}), filePath);
			}
			this.filePath = filePath;
			fileDirectory = Path.GetDirectoryName(filePath);
		}

		public Stream ReadFile()
		{
			return File.OpenRead(filePath);
		}
	}
}
