namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal sealed class MacStorageBuilder : IStorageBuilder
	{
		public StorageBase Create(string persistenceFolderName)
		{
			return new MacStorage(persistenceFolderName);
		}
	}
}
