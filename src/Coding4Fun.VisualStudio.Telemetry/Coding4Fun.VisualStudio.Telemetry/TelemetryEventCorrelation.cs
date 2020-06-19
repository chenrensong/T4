using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A struct to define correlation information.
	/// </summary>
	public struct TelemetryEventCorrelation
	{
		/// <summary>
		/// Represents empty <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation" />
		/// </summary>
		public static readonly TelemetryEventCorrelation Empty = new TelemetryEventCorrelation(Guid.Empty, DataModelEventType.Trace);

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		internal Guid Id
		{
			get;
			private set;
		}

		internal bool IsEmpty => Id == Guid.Empty;

		/// <summary>
		/// Gets event type of this correlation.
		/// </summary>
		/// <remarks>
		/// The value is used by backend server to improve query performance.
		/// </remarks>
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		[JsonConverter(typeof(StringEnumConverter))]
		internal DataModelEventType EventType
		{
			get;
			private set;
		}

		internal TelemetryEventCorrelation(Guid id, DataModelEventType eventType)
		{
			Id = id;
			EventType = eventType;
		}

		/// <summary>
		/// Serialize current object to string
		/// </summary>
		/// <returns>
		/// A json string to include id and event type properties.
		/// E.g., {"id":"d7149afe-632f-4278-b94e-3915a79ee60f","eventType":"Operation"}
		/// </returns>
		public string Serialize()
		{
			return JsonConvert.SerializeObject((object)this);
		}

		/// <summary>
		/// Deserialize a json string to <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation" /> object.
		/// </summary>
		/// <param name="jsonString">
		/// A json string to include id and event type properties.
		/// E.g., {"id":"d7149afe-632f-4278-b94e-3915a79ee60f","eventType":"Operation"}
		/// </param>
		/// <returns>
		/// A <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation" /> object.
		/// It would throw <see cref="T:Newtonsoft.Json.JsonReaderException" /> if parameter jsonString is an invalid Json string.
		/// It would throw <see cref="T:Newtonsoft.Json.JsonSerializationException" /> if any of conditions below happen,
		///     1. Miss properties "id" or "eventType"
		///     2. Invalid property value. "id" value should be a guid string, and "eventType" should be enum name from <see cref="T:Coding4Fun.VisualStudio.Telemetry.DataModelEventType" />.
		/// </returns>
		public static TelemetryEventCorrelation Deserialize(string jsonString)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(jsonString, "jsonString");
			return JsonConvert.DeserializeObject<TelemetryEventCorrelation>(jsonString);
		}
	}
}
