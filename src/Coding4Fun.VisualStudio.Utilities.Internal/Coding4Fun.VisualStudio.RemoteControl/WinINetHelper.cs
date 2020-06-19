using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// Provides a helper method to write empty files to the local IE cache. This is useful for writing HTTP error
	/// responses to the cache. Although caching of error responses is entirely allowed by HTTP protocol, The
	/// System.Net library does not add error responses to the cache. So, this helper library is used to call
	/// wininet.dll functions to do so.
	/// </summary>
	internal static class WinINetHelper
	{
		private const string CacheWriteTimestampHeaderName = "Cache-Write-Timestamp";

		private const int MAX_PATH = 260;

		private const uint NORMAL_CACHE_ENTRY = 1u;

		/// <summary>
		/// Adds an empty file to the local IE cache with the <paramref name="status" />header for the
		/// specified<paramref name="url" />.
		/// </summary>
		/// <param name="url">URL for which to add the cache entry</param>
		/// <param name="status">Status of the response to cache</param>
		/// <returns>True if operation succeeded. Otherwise false.</returns>
		internal static bool WriteErrorResponseToCache(string url, HttpStatusCode status)
		{
			if (status == HttpStatusCode.NotFound)
			{
				return WriteErrorResponseToCache(url, 404, "Not Found");
			}
			return WriteErrorResponseToCache(url, (int)status, "Unknown");
		}

		[DllImport("wininet", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "CreateUrlCacheEntryW", SetLastError = true)]
		private static extern bool CreateUrlCacheEntry(string lpszUrlName, uint dwExpectedFileSize, string lpszFileExtension, StringBuilder lpszFileName, uint dwReserved);

		[DllImport("wininet", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, EntryPoint = "CommitUrlCacheEntryW", SetLastError = true)]
		private static extern bool CommitUrlCacheEntry(string url, string fileName, System.Runtime.InteropServices.ComTypes.FILETIME ftExpiryTime, System.Runtime.InteropServices.ComTypes.FILETIME ftModifiedTime, uint cacheEntryType, string header, uint headerSize, string fileExt, string originalUrl);

		private static bool WriteErrorResponseToCache(string url, int statusCode, string statusCodeDescription)
		{
			string text = string.Format(CultureInfo.InvariantCulture, "HTTP/1.0 {0} {1}\r\n{2}: {3}\r\n\r\n", statusCode, statusCodeDescription, "Cache-Write-Timestamp", DateTime.Now.Ticks);
			StringBuilder stringBuilder = new StringBuilder
			{
				Capacity = 260
			};
			if (CreateUrlCacheEntry(url, 8u, "cache", stringBuilder, 0u))
			{
				return CommitUrlCacheEntry(url, stringBuilder.ToString(), default(System.Runtime.InteropServices.ComTypes.FILETIME), default(System.Runtime.InteropServices.ComTypes.FILETIME), 1u, text, (uint)text.Length, null, null);
			}
			return false;
		}
	}
}
