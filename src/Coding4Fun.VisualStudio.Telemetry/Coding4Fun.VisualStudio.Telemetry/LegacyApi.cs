using Coding4Fun.VisualStudio.Utilities.Internal;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class LegacyApi : ILegacyApi
	{
		private static readonly object userIdCalculation = new object();

		private const string MachineIdRegPath = "SOFTWARE\\Coding4Fun\\SQMClient";

		private const string MachineIdRegKey = "MachineId";

		private const string UserIdRegPath = "SOFTWARE\\Coding4Fun\\SQMClient";

		private const string UserIdRegKey = "UserId";

		private IRegistryTools registryTools;

		public LegacyApi(IRegistryTools registryTools)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			if (registryTools == null)
			{
				registryTools = (IRegistryTools)(object)new RegistryTools();
			}
			this.registryTools = registryTools;
		}

		public Guid ReadSharedMachineId()
		{
			Guid result = default(Guid);
			string text = ReadMachineIdFromRegistry(registryTools, "SOFTWARE\\Coding4Fun\\SQMClient", "MachineId");
			if (text != null)
			{
				try
				{
					result = new Guid(text);
					return result;
				}
				catch (FormatException)
				{
					return result;
				}
				catch (OverflowException)
				{
					return result;
				}
			}
			return result;
		}

		public bool SetSharedMachineId(Guid machineId)
		{
			return SaveMachineIdToRegistry(registryTools, "SOFTWARE\\Coding4Fun\\SQMClient", "MachineId", FormatGuid(machineId));
		}

		public Guid ReadSharedUserId()
		{
			Guid guid = default(Guid);
			lock (userIdCalculation)
			{
				string text = (string)registryTools.GetRegistryValueFromCurrentUserRoot("SOFTWARE\\Coding4Fun\\SQMClient", "UserId", (object)null);
				if (text != null)
				{
					try
					{
						guid = new Guid(text);
					}
					catch (FormatException)
					{
					}
					catch (OverflowException)
					{
					}
				}
				if (!(guid == default(Guid)))
				{
					return guid;
				}
				guid = Guid.NewGuid();
				if (!registryTools.SetRegistryFromCurrentUserRoot("SOFTWARE\\Coding4Fun\\SQMClient", "UserId", (object)FormatGuid(guid)))
				{
					return Guid.Empty;
				}
				return guid;
			}
		}

		protected virtual string ReadMachineIdFromRegistry(IRegistryTools registry, string regPath, string regKey)
		{
			return (string)registry.GetRegistryValueFromLocalMachineRoot(regPath, regKey, true, (object)null);
		}

		protected virtual bool SaveMachineIdToRegistry(IRegistryTools registry, string regPath, string regKey, string machineId)
		{
			return registry.SetRegistryFromLocalMachineRoot(regPath, regKey, (object)machineId, true);
		}

		private static string FormatGuid(Guid guid)
		{
			return guid.ToString("B").ToUpper();
		}
	}
}
