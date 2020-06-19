using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class InternalChannelValidator : IChannelValidator
	{
		private IUserInformationProvider userInformationProvider;

		public InternalChannelValidator(IUserInformationProvider theUserInformationProvider)
		{
			CodeContract.RequiresArgumentNotNull<IUserInformationProvider>(theUserInformationProvider, "theUserInformationProvider");
			userInformationProvider = theUserInformationProvider;
		}

		public bool IsValid(ISessionChannel channelToValidate)
		{
			CodeContract.RequiresArgumentNotNull<ISessionChannel>(channelToValidate, "channelToValidate");
			if ((channelToValidate.Properties & ChannelProperties.InternalOnly) != ChannelProperties.InternalOnly)
			{
				return true;
			}
			return userInformationProvider.IsUserMicrosoftInternal;
		}
	}
}
