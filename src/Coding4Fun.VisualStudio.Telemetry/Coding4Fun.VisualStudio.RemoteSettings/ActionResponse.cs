using Newtonsoft.Json;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class ActionResponse
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string ActionPath
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual int Precedence
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string RuleId
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string ActionType
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string FlightName
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string ActionJson
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets all the telemetry triggers that apply to this
		/// action. And example of a valid json would be:
		///
		///   {
		///     "start": {
		///         "triggerOnSubscribe": true
		///     },
		///     "stop": {
		///         "event": "some/telemetry/event"
		///     },
		///     "customEventName": {
		///         "triggerAlways": true
		///         "and": [
		///             { "event": "some/other/telemetry/event" },
		///             { "property": "some.property", "value": { "gt": 100 } }
		///         ]
		///     }
		///   }
		///
		/// This entire field will be deserialized into two types:
		///   IDictionary&lt;string, ITelemetryEventMatch$gt;
		///   IDictionary&lt;string, ActionTriggerOptions$gt;
		///
		/// There can be any number of triggers specified in the Json.
		/// "TriggerOnSubscribe" is only valid on the special "start"
		/// trigger. If "start" does not specify "TriggerOnSubscribe"
		/// then it must have a full valid ITelemetryEventMatch set of
		/// telemetry conditions like all other triggers.
		/// </summary>
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string TriggerJson
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual string MaxWaitTimeSpan
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual bool SendAlways
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public virtual IList<string> Categories
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string Origin
		{
			get;
			set;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Origin))
			{
				return RuleId;
			}
			return Origin + "-" + RuleId;
		}
	}
}
