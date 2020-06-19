using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class RegistryChannelValidator : IChannelValidator
	{
		private readonly IInternalSettings internalSettings;

		public RegistryChannelValidator(IInternalSettings internalSettings)
		{
			CodeContract.RequiresArgumentNotNull<IInternalSettings>(internalSettings, "internalSettings");
			this.internalSettings = internalSettings;
		}

		/// <summary>
		/// Check whether channel is valid to be added to active channel list
		/// </summary>
		/// <param name="channelToValidate"></param>
		/// <returns></returns>
		public bool IsValid(ISessionChannel channelToValidate)
		{
			CodeContract.RequiresArgumentNotNull<ISessionChannel>(channelToValidate, "channelToValidate");
			ChannelInternalSetting channelSettings = internalSettings.GetChannelSettings(channelToValidate.ChannelId);
			if (channelSettings == ChannelInternalSetting.ExplicitlyEnabled)
			{
				channelToValidate.Properties |= ChannelProperties.Default;
			}
			return channelSettings != ChannelInternalSetting.ExplicitlyDisabled;
		}
	}
}
