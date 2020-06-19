using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class TargetedNotificationsParser : ITargetedNotificationsParser
	{
		public async Task<ActionResponseBag> ParseStreamAsync(Stream stream)
		{
			string text = null;
			ActionResponseBag actions = new ActionResponseBag();
			try
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					text = await streamReader.ReadToEndAsync().ConfigureAwait(false);
				}
			}
			catch
			{
				return actions;
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					actions = JsonConvert.DeserializeObject<ActionResponseBag>(text);
				}
				catch (Exception ex)
				{
					throw new TargetedNotificationsException(ex.Message, ex);
				}
			}
			return actions;
		}
	}
}
