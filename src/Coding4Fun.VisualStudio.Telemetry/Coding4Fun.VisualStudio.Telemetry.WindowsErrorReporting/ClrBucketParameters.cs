using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// CLR description of Watson bucket parameters
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct ClrBucketParameters
	{
		public int FInited;

		/// <summary>
		/// EventName, "CLR20r3"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string PszEventTypeName;

		/// <summary>
		/// AppName "te.proesshost.managed.exe"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param0;

		/// <summary>
		/// AppVer "10.0.10132.0"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param1;

		/// <summary>
		/// AppStamp "556e0c0c"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param2;

		/// <summary>
		/// AsmAndModName "Coding4Fun.VisualStudio.Telemetry"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param3;

		/// <summary>
		/// Asmver "14.1.548.50964"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param4;

		/// <summary>
		/// ModStamp "564a8749"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param5;

		/// <summary>
		/// MethodDef "404"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param6;

		/// <summary>
		/// Offset "1d"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Param7;

		/// <summary>
		/// Param8 ExceptionType "System.ObjectDisposedException"
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string ExceptionType;

		/// <summary>
		/// Param9 Component ""
		/// </summary>
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string Component;

		/// <summary>
		/// these are the names as registered in the
		/// </summary>
		internal static string[] ClrBucketNames = new string[10]
		{
			"AppName",
			"AppVer",
			"AppStamp",
			"AsmAndModName",
			"AsmVer",
			"ModStamp",
			"MethodDef",
			"OffSet",
			"ExceptionType",
			"Component"
		};

		internal IEnumerable<KeyValuePair<string, string>> Parameters
		{
			get
			{
				yield return new KeyValuePair<string, string>(ClrBucketNames[0], Param0);
				yield return new KeyValuePair<string, string>(ClrBucketNames[1], Param1);
				yield return new KeyValuePair<string, string>(ClrBucketNames[2], Param2);
				yield return new KeyValuePair<string, string>(ClrBucketNames[3], Param3);
				yield return new KeyValuePair<string, string>(ClrBucketNames[4], Param4);
				yield return new KeyValuePair<string, string>(ClrBucketNames[5], Param5);
				yield return new KeyValuePair<string, string>(ClrBucketNames[6], Param6);
				yield return new KeyValuePair<string, string>(ClrBucketNames[7], Param7);
				yield return new KeyValuePair<string, string>(ClrBucketNames[8], ExceptionType);
				yield return new KeyValuePair<string, string>(ClrBucketNames[9], Component);
			}
		}
	}
}
