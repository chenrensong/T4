namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class WindowsProcessLockFactory : IProcessLockFactory
	{
		public IProcessLock CreateLocker(string folderFullName, string prefix)
		{
			folderFullName = folderFullName.Replace('\\', '_');
			return new WindowsProcessLock(prefix + folderFullName);
		}
	}
}
