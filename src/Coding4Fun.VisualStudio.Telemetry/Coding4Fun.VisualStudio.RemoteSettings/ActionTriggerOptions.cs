using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class ActionTriggerOptions
	{
		/// <summary>
		/// Gets or sets a value indicating whether the
		/// trigger should be invoked directly after
		/// subscribing or not.
		/// </summary>
		[JsonProperty(PropertyName = "triggerOnSubscribe")]
		public bool TriggerOnSubscribe
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the
		/// trigger should be invoked every time the
		/// given conditions are met, or only the first
		/// time.
		/// </summary>
		[JsonProperty(PropertyName = "triggerAlways")]
		public bool TriggerAlways
		{
			get;
			set;
		}
	}
}
