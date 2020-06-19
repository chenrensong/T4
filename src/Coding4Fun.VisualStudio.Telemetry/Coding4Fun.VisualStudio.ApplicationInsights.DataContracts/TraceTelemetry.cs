using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Telemetry type used for log messages.
	/// Contains a time and message and optionally some additional metadata.
	/// </summary>
	public sealed class TraceTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "Message";

		internal readonly string BaseType = typeof(MessageData).Name;

		internal readonly MessageData Data;

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
		/// Gets or sets the message text. For example, the text that would normally be written to a log file line.
		/// </summary>
		public string Message
		{
			get
			{
				return Data.message;
			}
			set
			{
				Data.message = value;
			}
		}

		/// <summary>
		/// Gets or sets Trace severity level.
		/// </summary>
		public SeverityLevel? SeverityLevel
		{
			get
			{
				return Data.severityLevel.TranslateSeverityLevel();
			}
			set
			{
				Data.severityLevel = value.TranslateSeverityLevel();
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this trace.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TraceTelemetry" /> class.
		/// </summary>
		public TraceTelemetry()
		{
			Data = new MessageData();
			context = new TelemetryContext(Data.properties, new Dictionary<string, string>());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TraceTelemetry" /> class.
		/// </summary>
		public TraceTelemetry(string message)
			: this()
		{
			Message = message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TraceTelemetry" /> class.
		/// </summary>
		public TraceTelemetry(string message, SeverityLevel severityLevel)
			: this(message)
		{
			SeverityLevel = severityLevel;
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
			Data.message = Data.message.SanitizeMessage();
			Data.message = Utils.PopulateRequiredStringValue(Data.message, "message", typeof(TraceTelemetry).FullName);
			Data.properties.SanitizeProperties();
		}
	}
}
