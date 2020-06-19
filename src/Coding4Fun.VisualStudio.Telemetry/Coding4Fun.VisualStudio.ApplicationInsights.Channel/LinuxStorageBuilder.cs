namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class LinuxStorageBuilder : IStorageBuilder
	{
		public StorageBase Create(string persistenceFolderName)
		{
			return new LinuxStorage(persistenceFolderName);
		}
	}
}
