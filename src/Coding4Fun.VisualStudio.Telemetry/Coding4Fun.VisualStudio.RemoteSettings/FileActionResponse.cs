using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class FileActionResponse : ActionResponse
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public override int Precedence
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public override string FlightName
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public override string RuleId
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public override string ActionType
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		[JsonConverter(typeof(TestActionJsonConverter))]
		public override string ActionJson
		{
			get;
			set;
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			RuleId = (RuleId ?? Guid.NewGuid().ToString());
			ActionType = (ActionType ?? string.Empty);
			FlightName = (FlightName ?? string.Empty);
		}
	}
}
