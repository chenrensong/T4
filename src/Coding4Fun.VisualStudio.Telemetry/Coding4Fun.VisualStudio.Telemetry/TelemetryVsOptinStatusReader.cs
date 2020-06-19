using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Globalization;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Read and set OptedIn value for the VS.
	/// </summary>
	internal sealed class TelemetryVsOptinStatusReader : ITelemetryOptinStatusReader
	{
		private enum OptinStatus
		{
			ReadFromGlobalPolicy,
			OptedInForAll,
			OptedOutForAll,
			OptedInForSomeOptedOutForSome,
			Undefined
		}

		private const string GlobalPolicyOptedInRegistryPath = "Software\\Policies\\Microsoft\\VisualStudio\\SQM";

		private const string LocalOptedInRegistryPath = "Software\\Microsoft\\VSCommon\\{0}\\SQM";

		private const string LocalOptedInRootRegistryPath = "Software\\Microsoft\\VSCommon";

		private const string OptedInRegistryKeyName = "OptIn";

		private const int UserIsOptedInValue = 1;

		private readonly IRegistryTools2 registryTools;

		public TelemetryVsOptinStatusReader(IRegistryTools2 registryTools)
		{
			CodeContract.RequiresArgumentNotNull<IRegistryTools2>(registryTools, "registryTools");
			this.registryTools = registryTools;
		}

		public bool ReadIsOptedInStatus(string productVersion)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(productVersion, "productVersion");
			bool result = false;
			if (TryGlobalPolicyOptedInStatus(out bool optedIn))
			{
				result = optedIn;
			}
			else
			{
				int? registryIntValueFromLocalMachineRoot = ((IRegistryTools)registryTools).GetRegistryIntValueFromLocalMachineRoot(string.Format(CultureInfo.InvariantCulture, "Software\\Microsoft\\VSCommon\\{0}\\SQM", new object[1]
				{
					productVersion
				}), "OptIn", (int?)null);
				if (registryIntValueFromLocalMachineRoot.HasValue)
				{
					result = (registryIntValueFromLocalMachineRoot.Value == 1);
				}
			}
			return result;
		}

		/// <summary>
		/// Calculate IsOptedIn status based on OptedIn status from all installed versions of VS.
		/// If all found OptedIn statuses are true we return true, otherwise we return false.
		/// </summary>
		/// <param name="session">Host telemetry session</param>
		/// <returns>OptedIn status</returns>
		public bool ReadIsOptedInStatus(TelemetrySession session)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(session, "session");
			bool flag = false;
			OptinStatus optinStatus = OptinStatus.Undefined;
			if (TryGlobalPolicyOptedInStatus(out bool optedIn))
			{
				flag = optedIn;
				optinStatus = OptinStatus.ReadFromGlobalPolicy;
			}
			else
			{
				bool flag2 = false;
				bool flag3 = false;
				string[] registrySubKeyNamesFromLocalMachineRoot = registryTools.GetRegistrySubKeyNamesFromLocalMachineRoot("Software\\Microsoft\\VSCommon", false);
				if (registrySubKeyNamesFromLocalMachineRoot != null && registrySubKeyNamesFromLocalMachineRoot.Length != 0)
				{
					string[] array = registrySubKeyNamesFromLocalMachineRoot;
					foreach (string text in array)
					{
						if (!KeyMatchesSqmFormat(text))
						{
							continue;
						}
						int? registryIntValueFromLocalMachineRoot = ((IRegistryTools)registryTools).GetRegistryIntValueFromLocalMachineRoot(string.Format(CultureInfo.InvariantCulture, "Software\\Microsoft\\VSCommon\\{0}\\SQM", new object[1]
						{
							text
						}), "OptIn", (int?)null);
						if (registryIntValueFromLocalMachineRoot.HasValue)
						{
							if (registryIntValueFromLocalMachineRoot.Value == 1)
							{
								flag2 = true;
							}
							else
							{
								flag3 = true;
							}
						}
					}
				}
				flag = (flag2 && !flag3);
				if (flag2 && !flag3)
				{
					optinStatus = OptinStatus.OptedInForAll;
				}
				else if (flag2 && flag3)
				{
					optinStatus = OptinStatus.OptedInForSomeOptedOutForSome;
				}
				else if (!flag2 && flag3)
				{
					optinStatus = OptinStatus.OptedOutForAll;
				}
			}
			session.PostProperty("vs.core.usevsisoptedinstatus", optinStatus.ToString());
			return flag;
		}

		/// <summary>
		/// Check whether subkey matches format XX.X, where X - is a digit.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private bool KeyMatchesSqmFormat(string key)
		{
			Version result;
			return Version.TryParse(key, out result);
		}

		private bool TryGlobalPolicyOptedInStatus(out bool optedIn)
		{
			optedIn = false;
			int? registryIntValueFromLocalMachineRoot = ((IRegistryTools)registryTools).GetRegistryIntValueFromLocalMachineRoot("Software\\Policies\\Microsoft\\VisualStudio\\SQM", "OptIn", (int?)null);
			if (registryIntValueFromLocalMachineRoot.HasValue)
			{
				optedIn = (registryIntValueFromLocalMachineRoot.Value == 1);
				return true;
			}
			return false;
		}
	}
}
