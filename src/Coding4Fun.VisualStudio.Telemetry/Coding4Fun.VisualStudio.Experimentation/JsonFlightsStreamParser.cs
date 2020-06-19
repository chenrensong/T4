using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal sealed class JsonFlightsStreamParser : IFlightsStreamParser
	{
		public async Task<T> ParseStreamAsync<T>(Stream stream) where T : IFlightsData
		{
			string text = null;
			T flights = default(T);
			try
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					text = await streamReader.ReadToEndAsync().ConfigureAwait(false);
				}
			}
			catch
			{
				return flights;
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					flights = JsonConvert.DeserializeObject<T>(text);
				}
				catch
				{
				}
			}
			return flights;
		}
	}
}
