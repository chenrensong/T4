using System;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IRemoteSettingsLogger : IDisposable
	{
		bool LoggingEnabled
		{
			get;
		}

		Task Start();

		void LogVerbose(string message);

		void LogVerbose(string message, object data);

		void LogInfo(string message);

		void LogError(string message);

		void LogError(string message, Exception exception);
	}
}
