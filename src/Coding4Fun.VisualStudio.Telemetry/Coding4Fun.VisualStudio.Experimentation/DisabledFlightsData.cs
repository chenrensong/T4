using Newtonsoft.Json;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Data Class to be used to deserialize response from Remote Control
	/// </summary>
	internal sealed class DisabledFlightsData : IFlightsData
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IList<string> Flights
		{
			get;
			set;
		}
	}
}
