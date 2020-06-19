using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class MacUserPropertyProvider : IPropertyProvider
	{
		private const string AdminValue = "Administrator";

		private const string NormalUserValue = "NormalUser";

		private readonly IUserInformationProvider userInfoProvider;

		private readonly Lazy<bool> adminInformation;

		public MacUserPropertyProvider(IUserInformationProvider theUserInfoProvider)
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
			telemetryContext.PostProperty("VS.Core.User.IsDomainMember", false);
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
			return false;
		}
	}
}
