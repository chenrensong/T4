using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface ITargetedNotificationsParser
	{
		Task<ActionResponseBag> ParseStreamAsync(Stream stream);
	}
}
