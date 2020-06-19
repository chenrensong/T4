using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal interface IFlightsStreamParser
	{
		Task<T> ParseStreamAsync<T>(Stream stream) where T : IFlightsData;
	}
}
