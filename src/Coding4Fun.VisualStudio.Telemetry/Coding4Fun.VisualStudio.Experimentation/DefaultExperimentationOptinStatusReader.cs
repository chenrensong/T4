using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal sealed class DefaultExperimentationOptinStatusReader : IExperimentationOptinStatusReader
	{
		private readonly TelemetrySession telemetrySession;

		private readonly IRegistryTools registryTools;

		private readonly Lazy<bool> isOptedIn;

		public bool IsOptedIn => isOptedIn.Value;

		public DefaultExperimentationOptinStatusReader(TelemetrySession telemetrySession, IRegistryTools registryTools)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(registryTools, "registryTools");
			this.telemetrySession = telemetrySession;
			this.registryTools = registryTools;
			isOptedIn = new Lazy<bool>(() => GetIsOptedIn());
		}

		private bool GetIsOptedIn()
		{
			TypeTools.TryConvertToInt(registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\Telemetry", "TurnOffSwitch", (object)0), out int result);
			bool flag = result == 1;
			if (telemetrySession.IsOptedIn)
			{
				return !flag;
			}
			return false;
		}
	}
}
