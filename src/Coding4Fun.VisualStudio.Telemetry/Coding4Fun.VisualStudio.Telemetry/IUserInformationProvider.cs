using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Check whether user is internal microsoft employee and whether he/she
	/// logged in from the internal microsoft network.
	/// This information is neccessary for sending PII.
	/// We can send PII data only for internal users. The only exception is the Europe users.
	/// </summary>
	internal interface IUserInformationProvider
	{
		/// <summary>
		/// Gets a value indicating whether the current session is deemed to be a session qualified
		/// to collect private information.
		/// </summary>
		bool CanCollectPrivateInformation
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating whether the current session is deemed to be a "Coding4Fun Internal" session
		/// </summary>
		bool IsUserMicrosoftInternal
		{
			get;
		}

		/// <summary>
		/// Gets a value indicating a unique ID for the current user.
		/// </summary>
		Guid UserId
		{
			get;
		}

		/// <summary>
		/// Gets a value for the user type.
		/// </summary>
		UserType UserType
		{
			get;
		}
	}
}
