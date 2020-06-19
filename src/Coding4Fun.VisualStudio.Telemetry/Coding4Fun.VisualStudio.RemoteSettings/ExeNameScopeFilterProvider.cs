using Coding4Fun.VisualStudio.Utilities.Internal;
using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class ExeNameScopeFilterProvider : ISingleValueScopeFilterProvider<StringScopeValue>, IScopeFilterProvider
	{
		private readonly Lazy<StringScopeValue> exeName;

		public string Name => "ExeName";

		public ExeNameScopeFilterProvider(RemoteSettingsFilterProvider filterProvider)
		{
			CodeContract.RequiresArgumentNotNull<RemoteSettingsFilterProvider>(filterProvider, "filterProvider");
			exeName = new Lazy<StringScopeValue>(delegate
			{
				string applicationName = filterProvider.GetApplicationName();
				return new StringScopeValue(string.IsNullOrEmpty(applicationName) ? "Unknown" : applicationName);
			});
		}

		public StringScopeValue Provide()
		{
			return exeName.Value;
		}
	}
}
