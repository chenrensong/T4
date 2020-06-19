using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class RemoteSettingsStorageHandlerFactory : IRemoteSettingsStorageHandlerFactory
	{
		private const string PathPrefix = "Software\\Coding4Fun\\VisualStudio\\RemoteSettings";

		private const string PathFormatWithPrefix = "{0}\\{1}";

		private const string IsDisabledName = "TurnOffSwitch";

		private const string IsLoggingEnabled = "LoggingEnabled";

		private const int RemoteSettingsExplicitlyDisabled = 1;

		private const int RemoteSettingsLoggingEnabled = 1;

		private readonly IRemoteSettingsLogger logger;

		private readonly Regex fileNameRegex = new Regex("^[a-zA-Z0-9_.-]+\\.json$");

		public RemoteSettingsStorageHandlerFactory(IRemoteSettingsLogger logger)
		{
			this.logger = logger;
		}

		public static Func<bool> BuildIsUpdateDisabled(ICollectionKeyValueStorage storage, bool usePrefix)
		{
			CodeContract.RequiresArgumentNotNull<ICollectionKeyValueStorage>(storage, "storage");
			string collectionPathPrefix = usePrefix ? "Software\\Coding4Fun\\VisualStudio\\RemoteSettings" : string.Empty;
			return () => storage.GetValue(collectionPathPrefix, "TurnOffSwitch", 0) == 1;
		}

		public static Func<bool> BuildIsLoggingEnabled(ICollectionKeyValueStorage storage, bool usePrefix)
		{
			CodeContract.RequiresArgumentNotNull<ICollectionKeyValueStorage>(storage, "storage");
			string collectionPathPrefix = usePrefix ? "Software\\Coding4Fun\\VisualStudio\\RemoteSettings" : string.Empty;
			return () => storage.GetValue(collectionPathPrefix, "LoggingEnabled", 0) == 1;
		}

		public IVersionedRemoteSettingsStorageHandler BuildVersioned(ICollectionKeyValueStorage storage, bool usePrefix, string fileName, IScopeParserFactory scopeParserFactory)
		{
			if (!fileNameRegex.IsMatch(fileName))
			{
				throw new ArgumentException("Filename is invalid", "fileName");
			}
			string collectionPathPrefix = (!usePrefix) ? fileName : string.Format("{0}\\{1}", "Software\\Coding4Fun\\VisualStudio\\RemoteSettings", fileName, CultureInfo.InvariantCulture);
			return new RemoteSettingsStorageHandler(storage, collectionPathPrefix, scopeParserFactory, true, logger);
		}

		public IRemoteSettingsStorageHandler Build(ICollectionKeyValueStorage storage, bool usePrefix, RemoteSettingsFilterProvider filterProvider, IScopeParserFactory scopeParserFactory)
		{
			CodeContract.RequiresArgumentNotNull<RemoteSettingsFilterProvider>(filterProvider, "filterProvider");
			List<string> list = new List<string>();
			list.AddIfNotEmpty(filterProvider.GetApplicationName());
			list.AddIfNotEmpty(filterProvider.GetApplicationVersion());
			list.AddIfNotEmpty(filterProvider.GetBranchBuildFrom());
			string text = string.Join("\\", list);
			string collectionPathPrefix = (!usePrefix) ? text : string.Format("{0}\\{1}", "Software\\Coding4Fun\\VisualStudio\\RemoteSettings", text, CultureInfo.InvariantCulture);
			return new RemoteSettingsStorageHandler(storage, collectionPathPrefix, scopeParserFactory, false, logger);
		}

		public IRemoteSettingsStorageHandler Build(ICollectionKeyValueStorage storage, bool usePrefix, string collectionPath, IScopeParserFactory scopeParserFactory)
		{
			string collectionPathPrefix = (!usePrefix) ? collectionPath : string.Format("{0}\\{1}", "Software\\Coding4Fun\\VisualStudio\\RemoteSettings", collectionPath, CultureInfo.InvariantCulture);
			return new RemoteSettingsStorageHandler(storage, collectionPathPrefix, scopeParserFactory, false, logger);
		}
	}
}
