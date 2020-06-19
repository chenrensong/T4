namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	internal interface IProcessInformationProvider
	{
		string GetExeName();

		FileVersion GetProcessVersionInfo();
	}
}
