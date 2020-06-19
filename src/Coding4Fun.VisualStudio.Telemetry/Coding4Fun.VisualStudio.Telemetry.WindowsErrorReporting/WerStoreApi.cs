using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// Managed wrappers around wer APIs added in Windows 10 RS2 that allow access to queued or archived wer reports
	/// </summary>
	public class WerStoreApi
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct ReportMetaData
		{
			public WER_REPORT_SIGNATURE Signature;

			public GUID BucketId;

			public GUID ReportId;

			public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;

			public ulong SizeInBytes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string CabID;

			public ulong ReportStatus;

			public GUID ReportIntegratorId;

			public uint NumberOfFiles;

			public uint SizeOfFileNames;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
			public string FileNames;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WER_REPORT_SIGNATURE
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
			public string EventName;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
			public WER_REPORT_PARAMETER[] Parameters;
		}

		/// <summary>
		/// Represents a single Watson bucket parameter.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WER_REPORT_PARAMETER
		{
			/// <summary>
			/// The bucket parameter name
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
			public string Name;

			/// <summary>
			/// The bucket Parameter value
			/// </summary>
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string Value;
		}

		private struct GUID
		{
			public int Data1;

			public short Data2;

			public short Data3;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] Data4;
		}

		/// <summary>
		/// Defines the types of Wer report store that can be opened.
		/// </summary>
		public enum REPORT_STORE_TYPES
		{
			/// <summary>
			/// Machine wide store of archived reports. You cannot depend on how long reports will be here.
			/// older reports are better obtained through the event log.
			/// </summary>
			MACHINE_ARCHIVE = 2,
			/// <summary>
			/// Machine wide store of queued reports.
			/// </summary>
			MACHINE_QUEUE
		}

		/// <summary>
		/// Allows you to iterate the available wer reports on the computer.
		/// </summary>
		public interface IWerStore : IDisposable
		{
			/// <summary>
			/// Gets an enumeration of the reports this store contains
			/// </summary>
			/// <returns>An enumeration of the reports in this store.</returns>
			IEnumerable<IWerReportData> GetReports();
		}

		private class EmptyStore : IWerStore, IDisposable
		{
			public IEnumerable<IWerReportData> GetReports()
			{
				return Enumerable.Empty<IWerReportData>();
			}

			public void Dispose()
			{
			}
		}

		/// <summary>
		/// Exposes information about a wer report.
		/// </summary>
		public interface IWerReportData
		{
			/// <summary>
			/// Gets the locally unique report ID. Should be present for all reports.
			/// </summary>
			Guid ReportId
			{
				get;
			}

			/// <summary>
			/// Gets the event Type the report was sent to.
			/// </summary>
			string EventType
			{
				get;
			}

			/// <summary>
			/// Gets an array of length 10 with the bucket parameters for this report
			/// </summary>
			WER_REPORT_PARAMETER[] Parameters
			{
				get;
			}

			/// <summary>
			/// Gets the cab ID, if present. This will be empty for reports that are not uploaded.
			/// </summary>
			string CabID
			{
				get;
			}

			/// <summary>
			/// Gets the bucket ID. This will be empty for reports that are not uploaded.
			/// </summary>
			Guid BucketId
			{
				get;
			}

			/// <summary>
			/// Gets the Timestamp of this report's creation
			/// </summary>
			DateTime TimeStamp
			{
				get;
			}
		}

		private class WerReportData : IWerReportData
		{
			private ReportMetaData reportInternal;

			public Guid ReportId => ToManagedGuid(reportInternal.ReportId);

			public string EventType => reportInternal.Signature.EventName;

			public WER_REPORT_PARAMETER[] Parameters => reportInternal.Signature.Parameters;

			public string CabID => reportInternal.CabID;

			public Guid BucketId => ToManagedGuid(reportInternal.BucketId);

			public DateTime TimeStamp => FiletimeToDateTime(reportInternal.CreationTime);

			public WerReportData(ReportMetaData data)
			{
				reportInternal = data;
			}
		}

		private class WerStore : IWerStore, IDisposable
		{
			private IntPtr hStore;

			private bool isDisposed;

			public WerStore(REPORT_STORE_TYPES type)
			{
				hStore = IntPtr.Zero;
				if (WerStoreOpen(type, ref hStore) != 0 || hStore == IntPtr.Zero)
				{
					throw new InvalidOperationException();
				}
				isDisposed = false;
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					WerStoreClose(hStore);
					isDisposed = true;
				}
			}

			public IEnumerable<IWerReportData> GetReports()
			{
				if (isDisposed)
				{
					throw new ObjectDisposedException("WerStore");
				}
				IEnumerable<string> enumerable = GetKeys().Reverse();
				foreach (string item in enumerable)
				{
					WerReportData report = GetReport(item);
					if (report != null)
					{
						yield return report;
					}
				}
			}

			private IEnumerable<string> GetKeys()
			{
				List<string> list = new List<string>();
				while (true)
				{
					IntPtr reportKeyPtr = IntPtr.Zero;
					WerStoreGetNextReportKey(hStore, ref reportKeyPtr);
					if (reportKeyPtr == IntPtr.Zero)
					{
						break;
					}
					string item = Marshal.PtrToStringUni(reportKeyPtr);
					list.Add(item);
				}
				return list;
			}

			private WerReportData GetReport(string key)
			{
				ReportMetaData report = default(ReportMetaData);
				switch (WerStoreQueryReportMetadataV2(hStore, key, ref report))
				{
				case 0u:
					return new WerReportData(report);
				case 2147942405u:
					return null;
				default:
					_ = -2147024774;
					return new WerReportData(report);
				}
			}
		}

		private static bool? doesInterfaceExist;

		private const string WerDllName = "wer.dll";

		/// <summary>
		/// Gets a value indicating whether the API is present. Should be present on RS2+.
		/// If it is not, opening a store will return null.
		/// </summary>
		public static bool IsStoreInterfacePresent
		{
			get
			{
				if (!doesInterfaceExist.HasValue)
				{
					IntPtr intPtr = LoadLibrary("wer.dll");
					IntPtr procAddress = GetProcAddress(intPtr, "WerStoreOpen");
					IntPtr procAddress2 = GetProcAddress(intPtr, "WerStoreQueryReportMetadataV2");
					IntPtr procAddress3 = GetProcAddress(intPtr, "WerStoreGetNextReportKey");
					doesInterfaceExist = (procAddress != IntPtr.Zero && procAddress2 != IntPtr.Zero && procAddress3 != IntPtr.Zero);
					FreeLibrary(intPtr);
				}
				return doesInterfaceExist.Value;
			}
		}

		[DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string fileName);

		[DllImport("Kernel32", SetLastError = true)]
		private static extern IntPtr FreeLibrary(IntPtr hLib);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern int WerStoreOpen(REPORT_STORE_TYPES storeType, ref IntPtr hResportStore);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern void WerStoreClose(IntPtr hResportStore);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern uint WerStoreGetNextReportKey(IntPtr hResportStore, ref IntPtr reportKeyPtr);

		[DllImport("wer.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern uint WerStoreQueryReportMetadataV2(IntPtr hResportStore, string reportKey, [MarshalAs(UnmanagedType.Struct)] ref ReportMetaData report);

		/// <summary>
		/// Opens a wer report store and allows you to enumerate the reports it contains.
		/// If called on an earlier operating system where the relevant APIs do not exist,
		/// it will return an empty enumerable.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IWerStore GetStore(REPORT_STORE_TYPES type)
		{
			if (IsStoreInterfacePresent)
			{
				try
				{
					return new WerStore(type);
				}
				catch (InvalidOperationException)
				{
					return new EmptyStore();
				}
			}
			return new EmptyStore();
		}

		private static Guid ToManagedGuid(GUID nativeGuid)
		{
			return new Guid(nativeGuid.Data1, nativeGuid.Data2, nativeGuid.Data3, nativeGuid.Data4);
		}

		private static DateTime FiletimeToDateTime(System.Runtime.InteropServices.ComTypes.FILETIME fileTime)
		{
			return DateTime.FromFileTimeUtc(((long)fileTime.dwHighDateTime << 32) | (uint)fileTime.dwLowDateTime);
		}
	}
}
