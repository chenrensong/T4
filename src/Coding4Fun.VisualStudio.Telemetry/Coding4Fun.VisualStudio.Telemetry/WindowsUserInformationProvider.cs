using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class WindowsUserInformationProvider : UserInformationProviderBase, IUserInformationProvider
	{
		private const string SqmUserTypeRegistryPath = "Software\\Policies\\Microsoft\\VisualStudio\\SQM";

		private const string SqmUserTypeRegistryKey = "UserType";

		private readonly IRegistryTools registryTools;

		private readonly Lazy<UserType> userType;

		/// <summary>
		/// Gets a value for the user type.
		/// </summary>
		public override UserType UserType => userType.Value;

		public WindowsUserInformationProvider(IRegistryTools regTools, IInternalSettings internalSettings, IEnvironmentTools envTools, ILegacyApi legacyApi, Guid? userId)
			: base(internalSettings, envTools, legacyApi, userId)
		{
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(regTools, "regTools");
			registryTools = regTools;
			userType = new Lazy<UserType>(() => CalculateUserType(), LazyThreadSafetyMode.ExecutionAndPublication);
		}

		private UserType CalculateUserType()
		{
			UserType result = UserType.External;
			object registryValueFromCurrentUserRoot = registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Policies\\Microsoft\\VisualStudio\\SQM", "UserType", (object)null);
			if (registryValueFromCurrentUserRoot != null && registryValueFromCurrentUserRoot is int && Enum.IsDefined(typeof(UserType), registryValueFromCurrentUserRoot))
			{
				result = (UserType)registryValueFromCurrentUserRoot;
			}
			return result;
		}
	}
}
