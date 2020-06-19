using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class MonoUserInformationProvider : UserInformationProviderBase, IUserInformationProvider
	{
		/// <summary>
		/// Gets user type, which is external by default as in VsLog.
		/// </summary>
		public override UserType UserType => UserType.External;

		public MonoUserInformationProvider(IInternalSettings internalSettings, IEnvironmentTools envTools, ILegacyApi legacyApi, Guid? userId)
			: base(internalSettings, envTools, legacyApi, userId)
		{
		}
	}
}
