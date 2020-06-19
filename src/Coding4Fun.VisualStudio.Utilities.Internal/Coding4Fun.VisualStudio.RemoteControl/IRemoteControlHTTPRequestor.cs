using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// Provides operation to issue HTTP requests to obtain a file, either from the server or from the local IE cache.
	/// </summary>
	internal interface IRemoteControlHTTPRequestor
	{
		/// <summary>
		/// Reads the file from the server url.
		/// In case of errors reading the file from the server, returned <see cref="T:Coding4Fun.VisualStudio.RemoteControl.GetFileResult" /> object's
		/// IsSuccessStatusCode value will be false.
		/// </summary>
		/// <returns>Information about the file obtained from the server</returns>
		Task<GetFileResult> GetFileFromServerAsync();

		/// <summary>
		/// Reads the file from the local IE cache only.
		/// If the file does not exist in the cache, the returned <see cref="T:Coding4Fun.VisualStudio.RemoteControl.GetFileResult" /> object's IsCached value
		/// will be false and Code will be Unused.
		/// </summary>
		/// <returns>Information about the file in the IE cache</returns>
		Task<GetFileResult> GetFileFromCacheAsync();

		/// <summary>
		/// Gets the elapsed time (in seconds) since the last error in downloading / revalidating the file from the
		/// server.
		/// </summary>
		/// <returns>Time in seconds since last error or Int.MaxValue if no error has ever occured.</returns>
		Task<int> LastServerRequestErrorSecondsAgoAsync();

		/// <summary>
		/// Cancels all in progress HTTP requests. Any future calls to this class should not be made.
		/// </summary>
		void Cancel();
	}
}
