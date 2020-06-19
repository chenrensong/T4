using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Serializes an object to a stream.
	/// </summary>
	internal interface IStreamSerializer
	{
		Task SerializeAsync(object objectToSerialize, TextWriter stream);
	}
}
