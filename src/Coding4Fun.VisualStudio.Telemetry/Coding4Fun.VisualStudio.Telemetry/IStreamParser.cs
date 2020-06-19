using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Parses an object from a stream.
	/// </summary>
	internal interface IStreamParser
	{
		Task<object> ParseAsync(TextReader stream);
	}
}
