using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// IRemoteSettings2 provides subscribable settings without code changes
	/// </summary>
	public interface IRemoteSettings2 : IDisposable
	{
		/// <summary>
		/// Subscribes to triggered remote actions of type T on the given action path.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="actionPath">Unique path to identify the actions to subscribe</param>
		/// <param name="callback">Callback to be invoked with each individual action when it becomes available</param>
		void SubscribeActions<T>(string actionPath, Action<ActionWrapper<T>> callback);

		/// <summary>
		/// Unsubscribes from triggered remote actions on the given action path
		/// </summary>
		/// <param name="actionPath">Unique path to identify the actions to unsubscribe</param>
		void UnsubscribeActions(string actionPath);
	}
}
