using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal class StorageTransmission : Transmission, IDisposable
	{
		/// <summary>
		/// Chunk size in bytes to convert to base64 string. Final size of the string should be less than or equal 85000 bytes.
		/// Final string length in bytes is: (30000 / 3) * 4 * 2 = 80000
		/// </summary>
		private const int ConvertChunkSize = 30000;

		private const int BufferSize = 40004;

		internal Action<StorageTransmission> Disposing;

		internal string FileName
		{
			get;
			private set;
		}

		internal string FullFilePath
		{
			get;
			private set;
		}

		protected StorageTransmission(string fullPath, Uri address, byte[] content, string contentType, string contentEncoding)
			: base(address, content, contentType, contentEncoding)
		{
			FullFilePath = fullPath;
			FileName = Path.GetFileName(fullPath);
		}

		/// <summary>
		/// Disposing the storage transmission.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public override string ToString()
		{
			return "FileName: " + FileName + ", ContentHash: " + base.ContentHash;
		}

		/// <summary>
		/// Creates a new transmission from the specified <paramref name="stream" />.
		/// </summary>
		/// <returns>Return transmission loaded from file; return null if the file is corrupted.</returns>
		internal static async Task<StorageTransmission> CreateFromStreamAsync(Stream stream, string fileName)
		{
			StreamReader reader = new StreamReader(stream);
			return new StorageTransmission(address: await ReadAddressAsync(reader).ConfigureAwait(false), contentType: await ReadHeaderAsync(reader, "Content-Type").ConfigureAwait(false), contentEncoding: await ReadHeaderAsync(reader, "Content-Encoding").ConfigureAwait(false), fullPath: fileName, content: await ReadContentAsync(reader).ConfigureAwait(false));
		}

		/// <summary>
		/// Saves the transmission to the specified <paramref name="stream" />.
		/// </summary>
		/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> representing the asynchronous operation.</returns>
		internal static async Task SaveAsync(Transmission transmission, Stream stream)
		{
			StreamWriter writer = new StreamWriter(stream);
			try
			{
				await writer.WriteLineAsync(transmission.EndpointAddress.ToString()).ConfigureAwait(false);
				await writer.WriteLineAsync("Content-Type:" + transmission.ContentType).ConfigureAwait(false);
				await writer.WriteLineAsync("Content-Encoding:" + transmission.ContentEncoding).ConfigureAwait(false);
				await writer.WriteLineAsync(string.Empty).ConfigureAwait(false);
				if (transmission.Content.Length * 8 / 3 < 80000)
				{
					await writer.WriteAsync(Convert.ToBase64String(transmission.Content)).ConfigureAwait(false);
				}
				else
				{
					char[] buffer = new char[40004];
					for (int i = 0; i < transmission.Content.Length; i += 30000)
					{
						int length = Math.Min(30000, transmission.Content.Length - i);
						int count = Convert.ToBase64CharArray(transmission.Content, i, length, buffer, 0);
						await writer.WriteAsync(buffer, 0, count).ConfigureAwait(false);
					}
				}
			}
			finally
			{
				writer.Flush();
			}
		}

		private static async Task<string> ReadHeaderAsync(TextReader reader, string headerName)
		{
			string text = await reader.ReadLineAsync().ConfigureAwait(false);
			if (string.IsNullOrEmpty(text))
			{
				throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} header is expected.", new object[1]
				{
					headerName
				}));
			}
			string[] array = text.Split(':');
			if (array.Length != 2)
			{
				throw new FormatException(string.Format(CultureInfo.InvariantCulture, "Unexpected header format. {0} header is expected. Actual header: {1}", new object[2]
				{
					headerName,
					text
				}));
			}
			if (array[0] != headerName)
			{
				throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} header is expected. Actual header: {1}", new object[2]
				{
					headerName,
					text
				}));
			}
			return array[1].Trim();
		}

		private static async Task<Uri> ReadAddressAsync(TextReader reader)
		{
			string obj = await reader.ReadLineAsync().ConfigureAwait(false);
			if (string.IsNullOrEmpty(obj))
			{
				throw new FormatException("Transmission address is expected.");
			}
			return new Uri(obj);
		}

		private static async Task<byte[]> ReadContentAsync(TextReader reader)
		{
			string text = await reader.ReadToEndAsync().ConfigureAwait(false);
			if (string.IsNullOrEmpty(text) || text == Environment.NewLine)
			{
				throw new FormatException("Content is expected.");
			}
			return Convert.FromBase64String(text);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				Disposing?.Invoke(this);
			}
		}
	}
}
