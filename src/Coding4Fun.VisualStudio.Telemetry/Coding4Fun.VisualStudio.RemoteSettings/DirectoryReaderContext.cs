using System.IO;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class DirectoryReaderContext
	{
		public string DirectoryName
		{
			get;
			set;
		}

		public string FileName
		{
			get;
			set;
		}

		public Stream Stream
		{
			get;
			set;
		}
	}
}
