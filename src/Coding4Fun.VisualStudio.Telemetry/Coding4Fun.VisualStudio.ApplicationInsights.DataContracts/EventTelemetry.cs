using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Telemetry type used to track events.
	/// </summary>
	public sealed class EventTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "Event";

		internal readonly string BaseType = typeof(EventData).Name;

		internal readonly EventData Data;

		private readonly TelemetryContext context;

		/// <summary>
		/// Gets or sets date and time when event was recorded.
		/// </summary>
		public DateTimeOffset Timestamp
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the value that defines absolute order of the telemetry item.
		/// </summary>
		public string Sequence
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the context associated with the current telemetry item.
		/// </summary>
		public TelemetryContext Context => context;

		/// <summary>
		/// Gets or sets the name of the event.
		/// </summary>
		public string Name
		{
			get
			{
				return Data.name;
			}
			set
			{
				Data.name = value;
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined event metrics.
		/// </summary>
		public IDictionary<string, double> Metrics => Data.measurements;

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this event.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.EventTelemetry" /> class.
		/// </summary>
		public EventTelemetry()
		{
			Data = new EventData();
			context = new TelemetryContext(Data.properties, new Dictionary<string, string>());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.EventTelemetry" /> class with the given <paramref name="name" />.
		/// </summary>
		/// <exception cref="T:System.ArgumentException">The event <paramref name="name" /> is null or empty string.</exception>
		public EventTelemetry(string name)
			: this()
		{
			Name = name;
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
			Name = Name.SanitizeName();
			Name = Utils.PopulateRequiredStringValue(Name, "name", typeof(EventTelemetry).FullName);
			Properties.SanitizeProperties();
			Metrics.SanitizeMeasurements();
		}
	}
}
