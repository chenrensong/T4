using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryTextWriter : ITelemetryWriter, IDisposable
	{
		private TextWriter writer;

		/// <summary>
		/// Initializes the internal writer
		/// </summary>
		/// <param name="filePath"></param>
		public TelemetryTextWriter(string filePath)
		{
			try
			{
				StreamWriter streamWriter = File.CreateText(filePath);
				streamWriter.AutoFlush = true;
				writer = TextWriter.Synchronized(streamWriter);
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		/// <summary>
		/// Writes a single line of text
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public async Task WriteLineAsync(string text)
		{
			if (writer != StreamWriter.Null)
			{
				try
				{
					await writer.WriteLineAsync(text);
				}
				catch (ObjectDisposedException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
		}

		/// <summary>
		/// Closes the writer.
		/// </summary>
		public void Dispose()
		{
			TextWriter textWriter = writer;
			if (textWriter != null && textWriter != StreamWriter.Null && Interlocked.CompareExchange(ref writer, StreamWriter.Null, textWriter) == textWriter)
			{
				textWriter.Dispose();
			}
		}
	}
}
