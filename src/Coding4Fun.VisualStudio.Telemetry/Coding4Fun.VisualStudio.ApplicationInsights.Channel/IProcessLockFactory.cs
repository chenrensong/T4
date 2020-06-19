namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal interface IProcessLockFactory
	{
		IProcessLock CreateLocker(string folderFullName, string prefix);
	}
}
