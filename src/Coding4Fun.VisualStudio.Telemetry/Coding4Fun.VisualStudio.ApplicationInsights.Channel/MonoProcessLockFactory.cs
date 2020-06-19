using System.IO;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class MonoProcessLockFactory : IProcessLockFactory
	{
		public IProcessLock CreateLocker(string folderFullName, string prefix)
		{
			if (string.IsNullOrEmpty(folderFullName))
			{
				return null;
			}
			return new MonoProcessLock(Path.Combine(folderFullName, "storage-lock"));
		}
	}
}
