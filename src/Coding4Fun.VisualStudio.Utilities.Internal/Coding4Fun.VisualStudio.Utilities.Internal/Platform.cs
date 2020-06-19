using System.IO;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
	/// <summary>
	/// Determines which platform the process is running on.
	/// </summary>
	public static class Platform
	{
		/// <summary>
		/// Returns true on Windows platform.
		/// </summary>
		public static readonly bool IsWindows;

		/// <summary>
		/// Returns true on Mac OS platforms.
		/// </summary>
		public static readonly bool IsMac;

		/// <summary>
		/// Returns true on Linux platforms.
		/// </summary>
		public static readonly bool IsLinux;

		static Platform()
		{
			IsWindows = (Path.DirectorySeparatorChar == '\\');
			IsMac = (!IsWindows && MacNativeMethods.IsRunningOnMac());
			IsLinux = (!IsMac && !IsWindows);
		}

		public static void Initialize()
		{
		}
	}
}
