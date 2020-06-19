namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal static class ActionWrapperEx
	{
		public static ActionWrapper<T> WithSubscriptionDetails<T>(this ActionWrapper<T> baseAction, ActionSubscriptionDetails details)
		{
			return new ActionWrapper<T>
			{
				RuleId = baseAction.RuleId,
				FlightName = baseAction.FlightName,
				ActionPath = baseAction.ActionPath,
				Precedence = baseAction.Precedence,
				Action = baseAction.Action,
				Subscription = details
			};
		}
	}
}
