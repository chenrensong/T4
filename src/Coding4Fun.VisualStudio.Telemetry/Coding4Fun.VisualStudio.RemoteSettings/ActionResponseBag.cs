using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class ActionResponseBag
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string ProductName
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<ActionResponse> Actions
		{
			get;
			set;
		} = Enumerable.Empty<ActionResponse>();


		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<ActionCategory> Categories
		{
			get;
			set;
		} = Enumerable.Empty<ActionCategory>();

	}
}
