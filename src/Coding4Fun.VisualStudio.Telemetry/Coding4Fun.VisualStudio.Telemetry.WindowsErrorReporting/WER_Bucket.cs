namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// The bucket parameters https://msdn.microsoft.com/en-us/library/windows/desktop/bb513626(v=vs.85).aspx
	/// this is 0 based, except in event log, where it's 1 based.
	/// </summary>
	public enum WER_Bucket
	{
		/// <summary>
		/// includes file extension
		/// WER_P0 AppName "te.processhost.managed.exe"
		/// </summary>
		WER_P0,
		/// <summary>
		/// WER_P1 AppVer "10.0.10132.0"
		/// </summary>
		WER_P1,
		/// <summary>
		/// WER_P2 AppStamp "556e0c0c"
		/// </summary>
		WER_P2,
		/// <summary>
		/// WER_P3 AsmAndModName "Coding4Fun.VisualStudio.Telemetry". No Extension
		/// </summary>
		WER_P3,
		/// <summary>
		/// WER_P4 Asmver "14.1.548.50964"
		/// </summary>
		WER_P4,
		/// <summary>
		/// WER_P5 ModStamp "564a8749"
		/// </summary>
		WER_P5,
		/// <summary>
		/// WER_P6 MethodDef "404"
		/// </summary>
		WER_P6,
		/// <summary>
		/// WER_P7 Offset "1d"
		/// </summary>
		WER_P7,
		/// <summary>
		/// WER_P8 ExceptionType "System.ObjectDisposedException"
		/// </summary>
		WER_P8,
		/// <summary>
		/// WER_P9 Component
		/// </summary>
		WER_P9
	}
}
