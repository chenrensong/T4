using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Globalization;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Implementation of some default remote settings filters that all apps should be able to take advantage of.
	/// </summary>
	public class DefaultRemoteSettingsFilterProvider : RemoteSettingsFilterProvider
	{
		private readonly TelemetrySession session;

		private readonly IProcessInformationProvider processInformationProvider;

		private readonly IOSInformationProvider osInformationProvider;

		/// <summary>
		/// Construct a default Remote Settings filter provider from the passed in TelemetrySession
		/// </summary>
		/// <param name="telemetrySession"></param>
		public DefaultRemoteSettingsFilterProvider(TelemetrySession telemetrySession)
			: this(telemetrySession, new ProcessInformationProvider(), new OSInformationProvider((IRegistryTools)(object)new RegistryTools()))
		{
		}//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown


		internal DefaultRemoteSettingsFilterProvider(TelemetrySession telemetrySession, IProcessInformationProvider processInformationProvider, IOSInformationProvider osInformationProvider)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			session = telemetrySession;
			CodeContract.RequiresArgumentNotNull<IProcessInformationProvider>(processInformationProvider, "processInformationProvider");
			this.processInformationProvider = processInformationProvider;
			CodeContract.RequiresArgumentNotNull<IOSInformationProvider>(osInformationProvider, "osInformationProvider");
			this.osInformationProvider = osInformationProvider;
		}

		/// <inheritdoc />
		public override Guid GetMachineId()
		{
			return session.MachineId;
		}

		/// <inheritdoc />
		public override Guid GetUserId()
		{
			return session.UserId;
		}

		/// <inheritdoc />
		public override string GetCulture()
		{
			return CultureInfo.CurrentUICulture.ToString();
		}

		/// <inheritdoc />
		public override string GetApplicationName()
		{
			return processInformationProvider.GetExeName() ?? string.Empty;
		}

		/// <inheritdoc />
		public override string GetApplicationVersion()
		{
			FileVersion processVersionInfo = processInformationProvider.GetProcessVersionInfo();
			if (processVersionInfo == null)
			{
				return string.Empty;
			}
			return processVersionInfo.ToString();
		}

		/// <inheritdoc />
		public override string GetMacAddressHash()
		{
			return session.MacAddressHash;
		}

		/// <inheritdoc />
		public override string GetOsType()
		{
			return "Windows";
		}

		/// <inheritdoc />
		public override string GetOsVersion()
		{
			return osInformationProvider.GetOSVersion();
		}

		/// <inheritdoc />
		public override bool GetIsUserInternal()
		{
			return session.IsUserMicrosoftInternal;
		}
	}
}
