using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal class ProcessInformationProvider : IProcessInformationProvider
	{
		[ExcludeFromCodeCoverage]
		public string GetExeName()
		{
			string fullProcessExeName = GetFullProcessExeName();
			if (!string.IsNullOrEmpty(fullProcessExeName))
			{
				return Path.GetFileNameWithoutExtension(fullProcessExeName).ToLowerInvariant();
			}
			return null;
		}

		[ExcludeFromCodeCoverage]
		public FileVersion GetProcessVersionInfo()
		{
			string fullProcessExeName = GetFullProcessExeName();
			if (!string.IsNullOrEmpty(fullProcessExeName))
			{
				try
				{
					return new FileVersion(FileVersionInfo.GetVersionInfo(fullProcessExeName));
				}
				catch (Exception)
				{
				}
			}
			return null;
		}

		[ExcludeFromCodeCoverage]
		private string GetFullProcessExeName()
		{
			StringBuilder stringBuilder = new StringBuilder(1000);
			GetModuleFileName(IntPtr.Zero, stringBuilder, stringBuilder.Capacity);
			string text = stringBuilder.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			return text;
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[ExcludeFromCodeCoverage]
		private static extern uint GetModuleFileName([In] IntPtr handleModule, [Out] StringBuilder filename, [In] [MarshalAs(UnmanagedType.U4)] int size);
	}
}
