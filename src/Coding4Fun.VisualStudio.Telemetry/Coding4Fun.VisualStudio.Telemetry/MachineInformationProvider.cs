using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class MachineInformationProvider : IMachineInformationProvider
	{
		private static readonly object machineIdCalculationLock = new object();

		private readonly Lazy<Guid> machineId;

		private readonly ILegacyApi legacyApi;

		private readonly IUserInformationProvider userInformationProvider;

		private readonly IMACInformationProvider macInformationProvider;

		/// <summary>
		/// Gets the unique ID for the machine (i.e. across users).
		/// </summary>
		public Guid MachineId => machineId.Value;

		public MachineInformationProvider(ILegacyApi legacyApi, IUserInformationProvider userInformationProvider, IMACInformationProvider macInformationProvider)
		{
			CodeContract.RequiresArgumentNotNull<ILegacyApi>(legacyApi, "legacyApi");
			CodeContract.RequiresArgumentNotNull<IUserInformationProvider>(userInformationProvider, "userInformationProvider");
			CodeContract.RequiresArgumentNotNull<IMACInformationProvider>(macInformationProvider, "macInformationProvider");
			this.legacyApi = legacyApi;
			this.userInformationProvider = userInformationProvider;
			this.macInformationProvider = macInformationProvider;
			machineId = new Lazy<Guid>(() => CalculateMachineId(), LazyThreadSafetyMode.ExecutionAndPublication);
		}

		private Guid CalculateMachineId()
		{
			lock (machineIdCalculationLock)
			{
				Guid guid = legacyApi.ReadSharedMachineId();
				if (!(guid == default(Guid)))
				{
					return guid;
				}
				string mACAddressHash = macInformationProvider.GetMACAddressHash();
				if (mACAddressHash == null)
				{
					guid = Guid.NewGuid();
					if (!legacyApi.SetSharedMachineId(guid))
					{
						return userInformationProvider.UserId;
					}
					return guid;
				}
				guid = ConvertHexHashToGuid(mACAddressHash);
				legacyApi.SetSharedMachineId(guid);
				return guid;
			}
		}

		private static Guid ConvertHexHashToGuid(string hex)
		{
			try
			{
				return new Guid(hex.Substring(0, 32));
			}
			catch
			{
			}
			return Guid.Empty;
		}
	}
}
