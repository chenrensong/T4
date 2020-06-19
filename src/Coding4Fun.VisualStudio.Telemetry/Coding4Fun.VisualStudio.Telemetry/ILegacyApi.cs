using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface ILegacyApi
	{
		Guid ReadSharedMachineId();

		bool SetSharedMachineId(Guid machineId);

		Guid ReadSharedUserId();
	}
}
