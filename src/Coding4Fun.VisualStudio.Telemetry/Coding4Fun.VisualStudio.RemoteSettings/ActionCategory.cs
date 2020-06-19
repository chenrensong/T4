using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class ActionCategory
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string CategoryId
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string WaitTimeSpan
		{
			get;
			set;
		}
	}
}
