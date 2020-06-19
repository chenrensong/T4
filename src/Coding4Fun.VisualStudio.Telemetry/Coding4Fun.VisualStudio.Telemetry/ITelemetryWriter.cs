using System;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ITelemetryWriter : IDisposable
	{
		/// <summary>
		/// Writes a single line of text
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		Task WriteLineAsync(string text);
	}
}
