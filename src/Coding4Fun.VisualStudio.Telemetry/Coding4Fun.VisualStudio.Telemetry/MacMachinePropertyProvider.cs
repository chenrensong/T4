using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static Coding4Fun.VisualStudio.Utilities.Internal.MacNativeMethods;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class MacMachinePropertyProvider : IPropertyProvider
	{
		private static readonly long MbInBytes = 1048576L;

		private const string NoneValue = "None";

		private const string UnknownValue = "Unknown";

		private const string MacAddressPropertyName = "VS.Core.MacAddressHash";

		private const string MachineIdPropertyName = "VS.Core.Machine.Id";

		private readonly Lazy<PerformanceCounter> memoryInformation;

		private readonly Lazy<SystemInfo> systemInformation;

		private readonly IMachineInformationProvider machineInformationProvider;

		private readonly IMACInformationProvider macInformationProvider;

		public MacMachinePropertyProvider(IMachineInformationProvider machineInformationProvider, IRegistryTools regTools, IMACInformationProvider macInformationProvider)
		{
			CodeContract.RequiresArgumentNotNull<IMachineInformationProvider>(machineInformationProvider, "machineInformationProvider");
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(regTools, "regTools");
			CodeContract.RequiresArgumentNotNull<IMACInformationProvider>(macInformationProvider, "macInformationProvider");
			this.machineInformationProvider = machineInformationProvider;
			this.macInformationProvider = macInformationProvider;
			memoryInformation = new Lazy<PerformanceCounter>(() => InitializeOSMemoryInformation(), false);
			systemInformation = new Lazy<SystemInfo>(() => InitializeSystemInformation(), false);
		}

		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.MacAddressHash", macInformationProvider.GetMACAddressHash()));
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.Machine.Id", machineInformationProvider.MachineId));
			macInformationProvider.RunProcessIfNecessary(delegate(string macAddress)
			{
				telemetryContext.SharedProperties["VS.Core.MacAddressHash"] = macAddress;
			});
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.Machine.TotalRAM", memoryInformation.Value.RawValue / MbInBytes);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.Machine.Processor.Architecture", systemInformation.Value.HardwareMachine);
			if (!token.IsCancellationRequested)
			{
                telemetryContext.PostProperty("VS.Core.Machine.Processor.Count", systemInformation.Value.HardwarePhysicalCpuSize);
				if (!token.IsCancellationRequested)
				{
					telemetryContext.PostProperty("VS.Core.Machine.Processor.Description", systemInformation.Value.MachineCpuBrandString);
					_ = token.IsCancellationRequested;
				}
			}
		}

		/// <summary>
		/// Initialize OS Total Physical Memory information
		/// We get this information from Mono's PerformanceCounters
		/// </summary>
		/// <returns></returns>
		private PerformanceCounter InitializeOSMemoryInformation()
		{
			return new PerformanceCounter("Mono Memory", "Total Physical Memory");
		}

		/// <summary>
		/// Initialize System information
		/// We get this information from Win API call to GetNativeSystemInfo
		/// </summary>
		/// <returns></returns>
		private SystemInfo InitializeSystemInformation()
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			SystemInfo result = default(SystemInfo);
			MacNativeMethods.GetSystemInfo(ref result);
			return result;
		}
	}
}
