namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Creates a StorageBase
	/// </summary>
	internal interface IStorageBuilder
	{
		StorageBase Create(string persistenceFolderName);
	}
}
