namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class RemoteSettingsProviderResult<T>
	{
		public bool RetrievalSuccessful
		{
			get;
			set;
		}

		public T Value
		{
			get;
			set;
		}
	}
}
