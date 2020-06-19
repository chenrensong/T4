using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class WindowsUserPropertyProvider : IPropertyProvider
	{
		private const string AdminValue = "Administrator";

		private const string NormalUserValue = "NormalUser";

		private readonly IUserInformationProvider userInfoProvider;

		private readonly Lazy<bool> adminInformation;

		public WindowsUserPropertyProvider(IUserInformationProvider theUserInfoProvider)
		{
			CodeContract.RequiresArgumentNotNull<IUserInformationProvider>(theUserInfoProvider, "theUserInfoProvider");
			userInfoProvider = theUserInfoProvider;
			adminInformation = new Lazy<bool>(() => InitializeAdminInformation(), false);
		}

		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.User.Id", userInfoProvider.UserId));
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.User.IsMicrosoftInternal", userInfoProvider.IsUserMicrosoftInternal));
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.User.Location.GeoId", RegionInfo.CurrentRegion.GeoId));
			sharedProperties.Add(new KeyValuePair<string, object>("VS.Core.User.Type", userInfoProvider.UserType.ToString()));
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				return;
			}
			telemetryContext.PostProperty("VS.Core.User.IsDomainMember", NativeMethods.IsOS(NativeMethods.OSFeatureFlag.OSDomainMember));
			if (!token.IsCancellationRequested)
			{
				telemetryContext.PostProperty("VS.Core.User.Location.CountryName", RegionInfo.CurrentRegion.EnglishName);
				if (!token.IsCancellationRequested)
				{
					string propertyValue = adminInformation.Value ? "Administrator" : "NormalUser";
					telemetryContext.PostProperty("VS.Core.User.Privilege", propertyValue);
				}
			}
		}

		private bool InitializeAdminInformation()
		{
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			if (current == null)
			{
				return false;
			}
			if (new WindowsPrincipal(current).IsInRole(WindowsBuiltInRole.Administrator))
			{
				return true;
			}
			int returnLength = Marshal.SizeOf(typeof(int));
			IntPtr intPtr = Marshal.AllocHGlobal(returnLength);
			try
			{
				if (!NativeMethods.GetTokenInformation(current.Token, 18, intPtr, returnLength, out returnLength))
				{
					return false;
				}
				int num = Marshal.ReadInt32(intPtr);
				if (num == 2 || num == 3)
				{
					return true;
				}
				return false;
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
		}
	}
}
