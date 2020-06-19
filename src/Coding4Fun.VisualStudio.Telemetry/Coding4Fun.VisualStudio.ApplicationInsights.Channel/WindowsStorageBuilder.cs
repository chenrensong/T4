namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class WindowsStorageBuilder : IStorageBuilder
	{
		public StorageBase Create(string persistenceFolderName)
		{
			return new WindowsStorage(persistenceFolderName);
		}
	}
}
