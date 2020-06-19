using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class VersionScopeFilterProvider : IMultiValueScopeFilterProvider<DoubleScopeValue>, IScopeFilterProvider
	{
		private readonly DoubleScopeValue unknown = new DoubleScopeValue(-1.0);

		private readonly Lazy<FileVersion> fileVersionInfo;

		public string Name => "Version";

		public VersionScopeFilterProvider(RemoteSettingsFilterProvider filterProvider)
		{
			CodeContract.RequiresArgumentNotNull<RemoteSettingsFilterProvider>(filterProvider, "filterProvider");
			fileVersionInfo = new Lazy<FileVersion>(delegate
			{
				FileVersion.TryParse(filterProvider.GetApplicationVersion(), out FileVersion value);
				return value;
			});
		}

		public DoubleScopeValue Provide(string key)
		{
			FileVersion value = fileVersionInfo.Value;
			if (value != null)
			{
				switch (key.ToLowerInvariant())
				{
				case "major":
					return new DoubleScopeValue(value.FileMajorPart);
				case "minor":
					return new DoubleScopeValue(value.FileMinorPart);
				case "build":
					return new DoubleScopeValue(value.FileBuildPart);
				case "revision":
					return new DoubleScopeValue(value.FileRevisionPart);
				default:
					return unknown;
				}
			}
			return unknown;
		}
	}
}
