using System.IO;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// An instance of this class represents reading from a particular file from disk.
	/// </summary>
	internal interface IFileReader
	{
		Stream ReadFile();
	}
}
