using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Base type for the user information provider. Platform specific implementations can use this type to calculate generic properties.
	/// </summary>
	internal abstract class UserInformationProviderBase
	{
		private const string FullUserDomainEnvironmentKey = "USERDNSDOMAIN";

		private readonly Lazy<bool> canCollectPrivateInformation;

		private readonly Lazy<bool> isUserMicrosoftInternal;

		private readonly Lazy<Guid> userId;

		private readonly IEnvironmentTools environmentTools;

		private readonly IInternalSettings internalSettings;

		private readonly ILegacyApi legacyApi;

		private static readonly HashSet<string> CanCollectPrivateInformationDomainList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"redmond.corp.microsoft.com",
			"northamerica.corp.microsoft.com",
			"fareast.corp.microsoft.com",
			"ntdev.corp.microsoft.com",
			"wingroup.corp.microsoft.com",
			"southpacific.corp.microsoft.com",
			"wingroup.windeploy.ntdev.microsoft.com",
			"ddnet.microsoft.com"
		};

		private static readonly HashSet<string> MicrosoftInternalDomainList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"redmond.corp.microsoft.com",
			"northamerica.corp.microsoft.com",
			"fareast.corp.microsoft.com",
			"ntdev.corp.microsoft.com",
			"wingroup.corp.microsoft.com",
			"southpacific.corp.microsoft.com",
			"wingroup.windeploy.ntdev.microsoft.com",
			"ddnet.microsoft.com",
			"europe.corp.microsoft.com"
		};

		/// <summary>
		/// Gets a value indicating whether the current session is deemed to be a session qualifie to collect private information
		/// This information is neccessary for sending PII.
		/// We can send PII data only for internal users. The only exception is the Europe users.
		/// </summary>
		public bool CanCollectPrivateInformation => canCollectPrivateInformation.Value;

		/// <summary>
		/// Gets a value indicating whether the current session is deemed to be a "Microsoft Internal" session
		/// Check whether user is internal microsoft employee and whether he/she
		/// logged in from the internal microsoft network.
		/// </summary>
		public bool IsUserMicrosoftInternal => isUserMicrosoftInternal.Value;

		/// <summary>
		/// Gets a value indicating a unique ID for the current user.
		/// </summary>
		public Guid UserId => userId.Value;

		public abstract UserType UserType
		{
			get;
		}

		public UserInformationProviderBase(IInternalSettings internalSettings, IEnvironmentTools envTools, ILegacyApi legacyApi, Guid? userId)
		{
			CodeContract.RequiresArgumentNotNull<IInternalSettings>(internalSettings, "internalSettings");
			CodeContract.RequiresArgumentNotNull<IEnvironmentTools>(envTools, "envTools");
			CodeContract.RequiresArgumentNotNull<ILegacyApi>(legacyApi, "legacyApi");
			this.internalSettings = internalSettings;
			environmentTools = envTools;
			this.legacyApi = legacyApi;
			canCollectPrivateInformation = new Lazy<bool>(CalculateCanCollectPrivateInformation, LazyThreadSafetyMode.ExecutionAndPublication);
			isUserMicrosoftInternal = new Lazy<bool>(CalculateIsInternal, LazyThreadSafetyMode.ExecutionAndPublication);
			this.userId = new Lazy<Guid>(() => (!userId.HasValue) ? this.legacyApi.ReadSharedUserId() : userId.Value, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		private bool CalculateIsInternal()
		{
			if (internalSettings.IsForcedUserExternal())
			{
				return false;
			}
			if (UserType != 0)
			{
				return true;
			}
			return ValidateDomainInformation(MicrosoftInternalDomainList);
		}

		private bool CalculateCanCollectPrivateInformation()
		{
			if (internalSettings.IsForcedUserExternal())
			{
				return false;
			}
			return ValidateDomainInformation(CanCollectPrivateInformationDomainList);
		}

		private bool ValidateDomainInformation(HashSet<string> domainList)
		{
			string text = null;
			try
			{
				text = environmentTools.GetEnvironmentVariable("USERDNSDOMAIN");
			}
			catch (SecurityException)
			{
			}
			if (text == null)
			{
				text = internalSettings.GetIPGlobalConfigDomainName();
			}
			return domainList.Contains(text);
		}
	}
}
