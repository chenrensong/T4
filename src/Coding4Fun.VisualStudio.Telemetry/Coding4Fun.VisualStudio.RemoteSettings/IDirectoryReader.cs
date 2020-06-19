using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IDirectoryReader
	{
		int Priority
		{
			get;
		}

		IEnumerable<DirectoryReaderContext> ReadAllFiles();
	}
}
