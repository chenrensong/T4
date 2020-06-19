using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class GroupedRemoteSettings : Dictionary<string, RemoteSettingPossibilities>
	{
		public GroupedRemoteSettings(DeserializedRemoteSettings deserializedRemoteSettings, string origin)
		{
			foreach (RemoteSetting setting in deserializedRemoteSettings.Settings)
			{
				setting.Origin = origin;
				if (!TryGetValue(setting.Path, out RemoteSettingPossibilities value))
				{
					value = new RemoteSettingPossibilities();
					this[setting.Path] = value;
				}
				if (!value.TryGetValue(setting.Name, out List<RemoteSetting> value2))
				{
					value2 = new List<RemoteSetting>();
					value[setting.Name] = value2;
				}
				value2.Add(setting);
			}
		}

		/// <summary>
		/// Merges the passed in GroupedRemoteSettings from buckets into this instance.
		/// Identical settings in this instance will be overwritten by those in buckets.
		/// </summary>
		/// <param name="buckets"></param>
		/// <param name="logger"></param>
		public void Merge(GroupedRemoteSettings buckets, IRemoteSettingsLogger logger)
		{
			foreach (string key in buckets.Keys)
			{
				RemoteSettingPossibilities remoteSettingPossibilities = buckets[key];
				if (!TryGetValue(key, out RemoteSettingPossibilities value))
				{
					value = (base[key] = new RemoteSettingPossibilities());
				}
				foreach (string key2 in remoteSettingPossibilities.Keys)
				{
					List<RemoteSetting> list = remoteSettingPossibilities[key2];
					if (!value.TryGetValue(key2, out List<RemoteSetting> value2))
					{
						value2 = (value[key2] = new List<RemoteSetting>());
					}
					for (int num = list.Count - 1; num >= 0; num--)
					{
						RemoteSetting remoteSetting = list[num];
						int num2 = value2.FindIndex((RemoteSetting s) => s.Name == remoteSetting.Name && s.ScopeString == remoteSetting.ScopeString);
						if (num2 != -1)
						{
							logger.LogVerbose($"Overwriting RemoteSetting during merge: Old value from {value2[num2]}");
							logger.LogVerbose($"Overwriting RemoteSetting during merge: New value from {remoteSetting}");
							value2[num2] = remoteSetting;
						}
						else
						{
							value2.Insert(0, remoteSetting);
						}
					}
				}
			}
		}
	}
}
