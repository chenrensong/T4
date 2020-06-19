using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class WindowsMachinePropertyProvider : IPropertyProvider
	{
		/// <summary>
		/// Processors architecture list copied from winnt.h
		/// </summary>
		private enum ProcessorArchitectureType
		{
			ProcessorArchitectureIntel = 0,
			ProcessorArchitectureMips = 1,
			ProcessorArchitectureAlpha = 2,
			ProcessorArchitecturePpc = 3,
			ProcessorArchitectureShx = 4,
			ProcessorArchitectureArm = 5,
			ProcessorArchitectureIA64 = 6,
			ProcessorArchitectureAlpha64 = 7,
			ProcessorArchitectureMsil = 8,
			ProcessorArchitectureAmd64 = 9,
			ProcessorArchitectureIA32OnWin64 = 10,
			ProcessorArchitectureUnknown = 0xFFFF
		}

		private static readonly ulong MbInBytes = 1048576uL;

		private const string AzureVMImageNameKey = "AzureVMImageName";

		private const string HardwareDescriptionRegistryPath = "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0";

		private const string HardwareProcessNameRegistryKey = "ProcessorNameString";

		private const string HardwareCPUSpeedRegistryKey = "~MHz";

		private const string NoneValue = "None";

		private const string TelemetryLocalMachineRegistryPath = "SOFTWARE\\Microsoft\\VisualStudio\\Telemetry";

		private const string UnknownValue = "Unknown";

		private const string MacAddressPropertyName = "VS.Core.MacAddressHash";

		private const string MachineIdPropertyName = "VS.Core.Machine.Id";

		private readonly Dictionary<ProcessorArchitectureType, string> processorArchitectureName = new Dictionary<ProcessorArchitectureType, string>
		{
			{
				ProcessorArchitectureType.ProcessorArchitectureIntel,
				"Intel"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureMips,
				"MIPS"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureAlpha,
				"Alpha"
			},
			{
				ProcessorArchitectureType.ProcessorArchitecturePpc,
				"PPC"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureShx,
				"SHX"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureArm,
				"ARM"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureIA64,
				"IA64"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureAlpha64,
				"Alpha64"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureMsil,
				"MSIL"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureAmd64,
				"AMD64"
			},
			{
				ProcessorArchitectureType.ProcessorArchitectureIA32OnWin64,
				"IA32 on Win64"
			}
		};

		private readonly Lazy<NativeMethods.MemoryStatus> memoryInformation;

		private readonly Lazy<NativeMethods.SystemInfo> systemInformation;

		private readonly Lazy<string> processorDescription;

		private readonly Lazy<int?> processorFrequency;

		private readonly Lazy<string> azureVMImageName;

		private readonly IMachineInformationProvider machineInformationProvider;

		private readonly IRegistryTools registryTools;

		private readonly IMACInformationProvider macInformationProvider;

		private string ProcessorArchitecture
		{
			get
			{
				ProcessorArchitectureType processorArchitecture = (ProcessorArchitectureType)systemInformation.Value.ProcessorArchitecture;
				if (!processorArchitectureName.TryGetValue(processorArchitecture, out string value))
				{
					return "Unknown";
				}
				return value;
			}
		}

		public WindowsMachinePropertyProvider(IMachineInformationProvider machineInformationProvider, IRegistryTools regTools, IMACInformationProvider macInformationProvider)
		{
			CodeContract.RequiresArgumentNotNull<IMachineInformationProvider>(machineInformationProvider, "machineInformationProvider");
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(regTools, "regTools");
			CodeContract.RequiresArgumentNotNull<IMACInformationProvider>(macInformationProvider, "macInformationProvider");
			this.machineInformationProvider = machineInformationProvider;
			registryTools = regTools;
			this.macInformationProvider = macInformationProvider;
			memoryInformation = new Lazy<NativeMethods.MemoryStatus>(() => InitializeOSMemoryInformation(), false);
			systemInformation = new Lazy<NativeMethods.SystemInfo>(() => InitializeSystemInformation(), false);
			processorDescription = new Lazy<string>(() => InitializeProcessorDescription(), false);
			processorFrequency = new Lazy<int?>(() => registryTools.GetRegistryIntValueFromLocalMachineRoot("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", "~MHz", (int?)null), false);
			azureVMImageName = new Lazy<string>(() => InitializeAzureVMImageName(), false);
		}

		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.Machine.Id", machineInformationProvider.MachineId));
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.MacAddressHash", macInformationProvider.GetMACAddressHash()));
			macInformationProvider.RunProcessIfNecessary(delegate(string macAddress)
			{
				telemetryContext.SharedProperties["VS.Core.MacAddressHash"] = macAddress;
			});
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.Machine.TotalRAM", memoryInformation.Value.TotalPhys / MbInBytes);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.Machine.Processor.Architecture", ProcessorArchitecture);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.Machine.Processor.Count", systemInformation.Value.NumberOfProcessors);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.Machine.Processor.Description", processorDescription.Value);
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.Machine.Processor.Family", systemInformation.Value.ProcessorLevel);
			if (token.IsCancellationRequested)
			{
				return;
			}
			if (processorFrequency.Value.HasValue)
			{
				telemetryContext.PostProperty("VS.Core.Machine.Processor.Frequency", processorFrequency.Value);
				if (token.IsCancellationRequested)
				{
					return;
				}
			}
			telemetryContext.PostProperty("VS.Core.Machine.Processor.Model", systemInformation.Value.ProcessorRevision >> 8);
			if (!token.IsCancellationRequested)
			{
				telemetryContext.PostProperty("VS.Core.Machine.Processor.Stepping", systemInformation.Value.ProcessorRevision & 0xFF);
				if (!token.IsCancellationRequested)
				{
					telemetryContext.PostProperty("VS.Core.Machine.VM.AzureImage", azureVMImageName.Value);
				}
			}
		}

		/// <summary>
		/// Initialize OS Memory information
		/// We get this information from Win API call to GlobalMemoryStatusEx
		/// </summary>
		/// <returns></returns>
		private NativeMethods.MemoryStatus InitializeOSMemoryInformation()
		{
			NativeMethods.MemoryStatus memoryStatus = default(NativeMethods.MemoryStatus);
			memoryStatus.Length = (uint)Marshal.SizeOf(typeof(NativeMethods.MemoryStatus));
			NativeMethods.MemoryStatus bufferPointer = memoryStatus;
			NativeMethods.GlobalMemoryStatusEx(bufferPointer);
			return bufferPointer;
		}

		/// <summary>
		/// Initialize System information
		/// We get this information from Win API call to GetNativeSystemInfo
		/// </summary>
		/// <returns></returns>
		private NativeMethods.SystemInfo InitializeSystemInformation()
		{
			NativeMethods.SystemInfo systemInfo = default(NativeMethods.SystemInfo);
			NativeMethods.GetNativeSystemInfo(systemInfo);
			return systemInfo;
		}

		private string InitializeProcessorDescription()
		{
			object registryValueFromLocalMachineRoot = registryTools.GetRegistryValueFromLocalMachineRoot("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0", "ProcessorNameString", (object)null);
			if (registryValueFromLocalMachineRoot != null && registryValueFromLocalMachineRoot is string)
			{
				return (string)registryValueFromLocalMachineRoot;
			}
			return "Unknown";
		}

		private string InitializeAzureVMImageName()
		{
			object registryValueFromLocalMachineRoot = registryTools.GetRegistryValueFromLocalMachineRoot("SOFTWARE\\Microsoft\\VisualStudio\\Telemetry", "AzureVMImageName", (object)null);
			if (registryValueFromLocalMachineRoot != null && registryValueFromLocalMachineRoot is string)
			{
				return (string)registryValueFromLocalMachineRoot;
			}
			return "None";
		}
	}
}
