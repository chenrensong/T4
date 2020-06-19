namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// An action of type T that is defined on the TargetedNotifications backend.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ActionWrapper<T>
	{
		/// <summary>
		/// Gets the path under which this action lives.
		/// </summary>
		public string ActionPath
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the typed action.
		/// </summary>
		public T Action
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the precedence of actions of within the same ActionPath. Higher indicates higher precedence.
		/// </summary>
		public int Precedence
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets a unique identifier for the rule.
		/// </summary>
		public string RuleId
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets a name for the type of action.  Useful if consumer wants to handle processing their own actions.
		/// </summary>
		public string ActionType
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets an experimentation flight that needs to be enabled in order for this action to have been returned.
		/// </summary>
		public string FlightName
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets any subscription details for this action
		/// </summary>
		public ActionSubscriptionDetails Subscription
		{
			get;
			internal set;
		}

		internal string ActionJson
		{
			get;
			set;
		}
	}
}
