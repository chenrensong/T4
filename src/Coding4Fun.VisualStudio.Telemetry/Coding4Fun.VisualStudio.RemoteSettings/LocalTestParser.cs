using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class LocalTestParser : ILocalTestParser
	{
		public async Task<IEnumerable<ActionResponse>> ParseStreamAsync(DirectoryReaderContext streamContext)
		{
			string text = null;
			IEnumerable<ActionResponse> actions = Enumerable.Empty<ActionResponse>();
			try
			{
				using (StreamReader streamReader = new StreamReader(streamContext.Stream))
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
					JToken val = JToken.Parse(text);
					if (val is JArray)
					{
						actions = val.ToObject<IEnumerable<FileActionResponse>>();
					}
					else
					{
						if (!(val is JObject))
						{
							throw new JsonException("Stream was neither an array nor object");
						}
						actions = new ActionResponse[1]
						{
							val.ToObject<FileActionResponse>()
						};
					}
				}
				catch (Exception ex)
				{
					throw new TargetedNotificationsException(ex.Message, ex);
				}
			}
			foreach (ActionResponse item in actions)
			{
				item.Origin = streamContext.DirectoryName + "-" + streamContext.FileName;
			}
			return actions;
		}
	}
}
