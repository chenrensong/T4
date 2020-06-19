using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Encapsulates information about a web request handled by the application.
	/// </summary>
	/// <remarks>
	/// You can send information about requests processed by your web application to Application Insights by
	/// passing an instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.RequestTelemetry" /> class to the <see cref="M:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient.TrackRequest(Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.RequestTelemetry)" />
	/// method.
	/// </remarks>
	public sealed class RequestTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "Request";

		internal readonly string BaseType = typeof(RequestData).Name;

		internal readonly RequestData Data;

		private readonly TelemetryContext context;

		/// <summary>
		/// Gets or sets the date and time when request was processed by the application.
		/// </summary>
		public DateTimeOffset Timestamp
		{
			get
			{
				return ValidateDateTimeOffset(Data.startTime);
			}
			set
			{
				Data.startTime = value.ToString("o", CultureInfo.InvariantCulture);
			}
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
		/// Gets the object that contains contextual information about the application at the time when it handled the request.
		/// </summary>
		public TelemetryContext Context => context;

		/// <summary>
		/// Gets or sets the unique identifier of the request.
		/// </summary>
		public string Id
		{
			get
			{
				return Data.id;
			}
			set
			{
				Data.id = value;
			}
		}

		/// <summary>
		/// Gets or sets human-readable name of the requested page.
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
		/// Gets or sets response code returned by the application after handling the request.
		/// </summary>
		public string ResponseCode
		{
			get
			{
				return Data.responseCode;
			}
			set
			{
				Data.responseCode = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether application handled the request successfully.
		/// </summary>
		public bool Success
		{
			get
			{
				return Data.success;
			}
			set
			{
				Data.success = value;
			}
		}

		/// <summary>
		/// Gets or sets the amount of time it took the application to handle the request.
		/// </summary>
		public TimeSpan Duration
		{
			get
			{
				return Utils.ValidateDuration(Data.duration);
			}
			set
			{
				Data.duration = value.ToString();
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this request.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		/// <summary>
		/// Gets or sets request url (optional).
		/// </summary>
		public Uri Url
		{
			get
			{
				if (Data.url.IsNullOrWhiteSpace())
				{
					return null;
				}
				return new Uri(Data.url);
			}
			set
			{
				Data.url = ((value == null) ? null : value.ToString());
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined request metrics.
		/// </summary>
		public IDictionary<string, double> Metrics => Data.measurements;

		/// <summary>
		/// Gets or sets the HTTP method of the request.
		/// </summary>
		public string HttpMethod
		{
			get
			{
				return Data.httpMethod;
			}
			set
			{
				Data.httpMethod = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.RequestTelemetry" /> class.
		/// </summary>
		public RequestTelemetry()
		{
			Data = new RequestData();
			context = new TelemetryContext(Data.properties, new Dictionary<string, string>());
			Id = WeakConcurrentRandom.Instance.Next().ToString(CultureInfo.InvariantCulture);
			ResponseCode = "200";
			Success = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.RequestTelemetry" /> class with the given <paramref name="name" />,
		/// <paramref name="timestamp" />, <paramref name="duration" />, <paramref name="responseCode" /> and <paramref name="success" /> property values.
		/// </summary>
		public RequestTelemetry(string name, DateTimeOffset timestamp, TimeSpan duration, string responseCode, bool success)
			: this()
		{
			Name = name;
			Timestamp = timestamp;
			Duration = duration;
			ResponseCode = responseCode;
			Success = success;
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
			Name = Name.SanitizeName();
			Properties.SanitizeProperties();
			Metrics.SanitizeMeasurements();
			Url = Url.SanitizeUri();
			Id = Id.SanitizeName();
			Id = Utils.PopulateRequiredStringValue(Id, "id", typeof(RequestTelemetry).FullName);
			ResponseCode = Utils.PopulateRequiredStringValue(ResponseCode, "responseCode", typeof(RequestTelemetry).FullName);
		}

		private DateTimeOffset ValidateDateTimeOffset(string value)
		{
			if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTimeOffset result))
			{
				return DateTimeOffset.MinValue;
			}
			return result;
		}
	}
}
