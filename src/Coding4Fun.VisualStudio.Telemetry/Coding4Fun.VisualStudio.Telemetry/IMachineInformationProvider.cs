using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IMachineInformationProvider
	{
		/// <summary>
		/// Gets the unique ID for the machine (i.e. across users).
		/// </summary>
		Guid MachineId
		{
			get;
		}
	}
}
