using System;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// Interface for the client for the Remote Control Service.
	/// This client polls the service for a single settings file and keeps it up-to-date in the local IE cache (which
	/// is per-user). Developers may call the <see cref="M:Coding4Fun.VisualStudio.RemoteControl.IRemoteControlClient.ReadFile(Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale)" /> or
	/// <see cref="M:Coding4Fun.VisualStudio.RemoteControl.IRemoteControlClient.ReadFileAsync(Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale)" /> to read settings file.
	/// </summary>
	public interface IRemoteControlClient : IDisposable
	{
		/// <summary>
		/// Gets a full URL used to download to the file. Read-Only.
		/// </summary>
		string FullUrl
		{
			get;
		}

		/// <summary>
		/// Gets a polling Interval to check the file on the server. Read-Only.
		/// This setting also determines when a file in the local IE cache is
		/// considered stale.
		/// </summary>
		int PollingIntervalMins
		{
			get;
		}

		/// <summary>
		/// Reads the settings file based on the <paramref name="staleBehavior" /> specified.
		/// </summary>
		/// <param name="staleBehavior">See <see cref="T:Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale" /> for details about each possible setting.
		/// In most cases use the BehaviorOnStale.ReturnStale setting.
		/// !! WARNING about using BehaviorOnStale.ForceDownload !!
		/// * each server request is billed and costs add up from millions of clients.
		/// * the only acceptable use of ForceDownload is very infrequently and if you absolutely cannot function
		///   without the latest settings.
		/// * keep in mind that even once-per-process-lifetime may be too much if you process starts and stops very
		///   frequently.
		/// </param>
		/// <returns>A Stream that can be used to read the setting file. !Callers must call Dispose on this stream
		/// object returned. Or Null is returned in case of failure to get the file (or if server returned
		/// NotFound).</returns>
		Stream ReadFile(BehaviorOnStale staleBehavior);

		/// <summary>
		/// Reads the settings file based on the <paramref name="staleBehavior" /> specified. This is the Async version
		/// of ReadFile method.
		/// </summary>
		/// <param name="staleBehavior">See <see cref="T:Coding4Fun.VisualStudio.RemoteControl.BehaviorOnStale" /> for details about each possible setting.
		/// In most cases use the BehaviorOnStale.ReturnStale setting.</param>
		/// <returns>A Stream that can be used to read the setting file. !Callers must call Dispose on this stream
		/// object returned. Or Null is returned in case of failure to get the file (or if server returned
		/// NotFound).</returns>
		Task<Stream> ReadFileAsync(BehaviorOnStale staleBehavior);
	}
}
