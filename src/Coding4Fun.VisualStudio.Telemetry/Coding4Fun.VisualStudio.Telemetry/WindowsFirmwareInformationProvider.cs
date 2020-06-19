using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Provides access to firmware tables under windows OS.
	/// </summary>
	internal static class WindowsFirmwareInformationProvider
	{
		/// <summary>
		/// Gets the specific firmware table
		/// </summary>
		/// <param name="provider">The Firmware table provider</param>
		/// <param name="table">The Firmware table name</param>
		/// <returns>The firmware table as a byte array</returns>
		internal static byte[] GetSystemFirmwareTable(NativeMethods.FirmwareTableProviderSignature provider, string table)
		{
			if (table.Length != 4 || table.Any((char c) => c > '\u007f'))
			{
				throw new ArgumentException("Table names must consist of 4 Ascii charactors", "table");
			}
			int table2 = (int)(((uint)table[3] << 24) | ((uint)table[2] << 16) | ((uint)table[1] << 8) | table[0]);
			return GetSystemFirmwareTable(provider, table2);
		}

		/// <summary>
		/// Gets the specific firmware table
		/// </summary>
		/// <param name="provider">The Firmware table provider</param>
		/// <param name="table">The firmware table integer identifier</param>
		/// <returns>The firmware table as a byte array</returns>
		internal static byte[] GetSystemFirmwareTable(NativeMethods.FirmwareTableProviderSignature provider, int table)
		{
			int systemFirmwareTable = NativeMethods.GetSystemFirmwareTable(provider, table, IntPtr.Zero, 0);
			if (systemFirmwareTable <= 0)
			{
				throw new ExternalException($"There was an error obtaining the firmware table '{table}' from {provider}.", Marshal.GetLastWin32Error());
			}
			IntPtr intPtr = Marshal.AllocHGlobal(systemFirmwareTable);
			try
			{
				NativeMethods.GetSystemFirmwareTable(provider, table, intPtr, systemFirmwareTable);
				byte[] array = null;
				if (Marshal.GetLastWin32Error() == 0)
				{
					array = new byte[systemFirmwareTable];
					Marshal.Copy(intPtr, array, 0, systemFirmwareTable);
				}
				return array;
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}

		/// <summary>
		/// Enumerates all system firmware tables from the specified firmware table provider
		/// </summary>
		/// <param name="provider">The Firmware table provider</param>
		/// <returns>The table names</returns>
		internal static string[] EnumSystemFirmwareTables(NativeMethods.FirmwareTableProviderSignature provider)
		{
			int num = NativeMethods.EnumSystemFirmwareTables(provider, IntPtr.Zero, 0);
			if (num <= 0)
			{
				throw new ExternalException($"There was an error enumerating the firmware tables from {provider}.", Marshal.GetLastWin32Error());
			}
			byte[] array = new byte[num];
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			try
			{
				NativeMethods.EnumSystemFirmwareTables(provider, intPtr, num);
				Marshal.Copy(intPtr, array, 0, num);
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
			string[] array2 = new string[num / 4];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = Encoding.ASCII.GetString(array, 4 * i, 4);
			}
			return array2;
		}
	}
}
