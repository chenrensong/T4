using System;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	/// <summary>
	/// Interface to read the file asynchronously. It is up to the owner of the implementation
	/// to decide which file to read and from what location.
	/// </summary>
	internal interface IRemoteFileReader : IDisposable
	{
		Task<Stream> ReadFileAsync();
	}
}
