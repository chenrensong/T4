namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IHostInformationProvider
	{
		/// <summary>
		/// Gets the name of the current host process, for example devenv, in lowercase.
		/// </summary>
		string ProcessName
		{
			get;
		}

		uint ProcessId
		{
			get;
		}

		bool IsDebuggerAttached
		{
			get;
		}
	}
}
