namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// Specifies the behavior of the IRemoteControlClient.ReadFile method
	/// </summary>
	public enum BehaviorOnStale
	{
		/// <summary>
		/// Return the settings file in the local cache irrespective of staleness.
		/// </summary>
		ReturnStale,
		/// <summary>
		/// Return the settings file in the local cache if it is not stale. Otherwise return Null.
		/// </summary>
		ReturnNull,
		/// <summary>
		/// Revalidate or download the file from the server and return it. WARNING:
		/// * each server request is billed and costs add up from millions of clients.
		/// * the only acceptable use of ForceDownload is very infrequently and if you absolutely cannot function
		///   without the latest settings
		/// * keep in mind that even once-per-process-lifetime may be too much if you process starts and stops very
		///   frequently.
		/// </summary>
		ForceDownload
	}
}
