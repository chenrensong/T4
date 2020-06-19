using Coding4Fun.VisualStudio.Telemetry.SessionChannel;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IChannelValidator
	{
		bool IsValid(ISessionChannel channelToValidate);
	}
}
