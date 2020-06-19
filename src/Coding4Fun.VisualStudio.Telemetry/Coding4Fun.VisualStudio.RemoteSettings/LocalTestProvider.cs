using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class LocalTestProvider : TargetedNotificationsProviderBase
	{
		private readonly IEnumerable<IDirectoryReader> directories;

		private readonly ILocalTestParser localTestParser;

		public override string Name => "LocalTestTargetedNotifications";

		public LocalTestProvider(RemoteSettingsInitializer initializer)
			: base(initializer.LocalTestRemoteSettingsStorageHandler, initializer)
		{
			directories = initializer.LocalTestDirectories.OrderBy((IDirectoryReader x) => x.Priority);
			localTestParser = initializer.LocalTestParser;
		}

		protected override async Task<ActionResponseBag> GetTargetedNotificationActionsAsync()
		{
			IEnumerable<Task<IEnumerable<ActionResponse>>> tasks = directories.SelectMany((IDirectoryReader d) => d.ReadAllFiles()).Select(async delegate(DirectoryReaderContext x)
			{
				try
				{
					return await localTestParser.ParseStreamAsync(x);
				}
				catch (TargetedNotificationsException exception)
				{
					logger.LogError("Error parsing test file: " + x.DirectoryName + "-" + x.FileName, exception);
				}
				return Enumerable.Empty<ActionResponse>();
			});
			ActionResponseBag actionResponseBag = new ActionResponseBag();
			ActionResponseBag actionResponseBag2 = actionResponseBag;
			actionResponseBag2.Actions = (await Task.WhenAll(tasks)).SelectMany((IEnumerable<ActionResponse> x) => x);
			return actionResponseBag;
		}
	}
}
