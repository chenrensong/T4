namespace Coding4Fun.VisualStudio.Telemetry.Services
{
	/// <summary>
	/// Factory interface which create new instance of the IRemoteFileReader
	/// for customers to the use it to get remote file.
	/// </summary>
	internal interface IRemoteFileReaderFactory
	{
		IRemoteFileReader Instance();
	}
}
