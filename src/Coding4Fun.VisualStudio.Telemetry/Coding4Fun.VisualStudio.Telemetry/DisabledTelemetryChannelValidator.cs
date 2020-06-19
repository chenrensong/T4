using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class DisabledTelemetryChannelValidator : IChannelValidator
	{
		private readonly IInternalSettings internalSettings;

		public DisabledTelemetryChannelValidator(IInternalSettings internalSettings)
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
			return !internalSettings.IsTelemetryDisabledCompletely();
		}
	}
}
