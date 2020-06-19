using System.Diagnostics;
using System.Globalization;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal sealed class FileVersion
	{
		public int FileMajorPart
		{
			get;
			private set;
		}

		public int FileMinorPart
		{
			get;
			private set;
		}

		public int FileBuildPart
		{
			get;
			private set;
		}

		public int FileRevisionPart
		{
			get;
			private set;
		}

		public FileVersion(FileVersionInfo fileVersionInfo)
		{
			FileMajorPart = fileVersionInfo.FileMajorPart;
			FileMinorPart = fileVersionInfo.FileMinorPart;
			FileBuildPart = fileVersionInfo.FileBuildPart;
			FileRevisionPart = fileVersionInfo.FilePrivatePart;
		}

		public FileVersion(int major, int minor, int build, int revision)
		{
			FileMajorPart = major;
			FileMinorPart = minor;
			FileBuildPart = build;
			FileRevisionPart = revision;
		}

		public static bool TryParse(string fileVersion, out FileVersion value)
		{
			if (fileVersion != null)
			{
				string[] array = fileVersion.Split('.');
				if (array.Length == 4 && int.TryParse(array[0], out int result) && int.TryParse(array[1], out int result2) && int.TryParse(array[2], out int result3) && int.TryParse(array[3], out int result4))
				{
					value = new FileVersion(result, result2, result3, result4);
					return true;
				}
			}
			value = null;
			return false;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", FileMajorPart, FileMinorPart, FileBuildPart, FileRevisionPart);
		}
	}
}
