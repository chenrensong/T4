namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Interface for FaultCallback
	/// used for native code callers too
	/// </summary>
	public interface IFaultUtility
	{
		/// <summary>
		/// Each Watson dump includes a text file (ErrrorInformation.txt) with basic information such as Error.ToString,
		/// telemtry properties, etc.
		/// Call this to add information to the file
		/// </summary>
		/// <param name="information"></param>
		void AddErrorInformation(string information);

		/// <summary>
		/// Creates a process dump. The dump type (Full/Mini) is controlled by the Watson back end
		/// </summary>
		/// <param name="pid">The processid</param>
		void AddProcessDump(int pid);

		/// <summary>
		/// Include a file in the report
		/// </summary>
		/// <param name="fullpathname"></param>
		void AddFile(string fullpathname);

		/// <summary>
		/// Set the bucket parameter
		/// </summary>
		/// <param name="bucketNumber">Parameter number. 0 based: internally 0-9. Externally (in event log) the 1st param is P1)</param>
		/// <param name="value"></param>
		void SetBucketParameter(int bucketNumber, string value);

		/// <summary>
		/// Get the value for a particular bucket. Useful to get the values in the callback to see what they were set to
		/// </summary>
		/// <param name="bucketNumber"></param>
		/// <returns></returns>
		string GetBucketParameter(int bucketNumber);
	}
}
