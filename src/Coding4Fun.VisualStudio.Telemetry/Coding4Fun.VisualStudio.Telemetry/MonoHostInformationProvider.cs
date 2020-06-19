using System;
using System.Diagnostics;
using System.IO;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class MonoHostInformationProvider : IHostInformationProvider
	{
		private readonly Lazy<string> name = new Lazy<string>(InitializeName, false);

		private readonly Lazy<uint> id = new Lazy<uint>(InitializeId, false);

		private const string UnknownName = "unknown";

		/// <summary>
		/// Gets the name of the current host process, for example devenv, in lowercase.
		/// </summary>
		public string ProcessName => name.Value;

		/// <summary>
		/// Gets the process id of the current host process.
		/// </summary>
		public uint ProcessId => id.Value;

		public bool IsDebuggerAttached => Debugger.IsAttached;

		/// <summary>
		/// For Perf reasons (in order to reduce RefSet by about 400KB during VS Start) we use the
		/// Win32 API to read the Process Exe name instead of using the .NET API.
		/// </summary>
		/// <returns></returns>
		private static string InitializeName()
		{
			return Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName).ToLowerInvariant();
		}

		private static uint InitializeId()
		{
			return (uint)Process.GetCurrentProcess().Id;
		}
	}
}
