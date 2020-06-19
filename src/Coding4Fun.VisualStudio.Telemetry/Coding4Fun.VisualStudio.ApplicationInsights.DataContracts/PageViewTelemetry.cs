using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Telemetry type used to track page views.
	/// </summary>
	/// <remarks>
	/// You can send information about pages viewed by your application to Application Insights by
	/// passing an instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.PageViewTelemetry" /> class to the <see cref="M:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient.TrackPageView(Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.PageViewTelemetry)" />
	/// method.
	/// </remarks>
	public sealed class PageViewTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "PageView";

		internal readonly string BaseType = typeof(PageViewData).Name;

		internal readonly PageViewData Data;

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
		/// Gets or sets the name of the metric.
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
		/// Gets or sets the page view Uri.
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
				if (value == null)
				{
					Data.url = null;
				}
				else
				{
					Data.url = value.ToString();
				}
			}
		}

		/// <summary>
		/// Gets or sets the page view duration.
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
		/// Gets a dictionary of custom defined metrics.
		/// </summary>
		public IDictionary<string, double> Metrics => Data.measurements;

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this page view.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.PageViewTelemetry" /> class.
		/// </summary>
		public PageViewTelemetry()
		{
			Data = new PageViewData();
			context = new TelemetryContext(Data.properties, new Dictionary<string, string>());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.PageViewTelemetry" /> class with the
		/// specified <paramref name="pageName" />.
		/// </summary>
		/// <exception cref="T:System.ArgumentException">The <paramref name="pageName" /> is null or empty string.</exception>
		public PageViewTelemetry(string pageName)
			: this()
		{
			Name = pageName;
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
			Name = Name.SanitizeName();
			Name = Utils.PopulateRequiredStringValue(Name, "name", typeof(PageViewTelemetry).FullName);
			Properties.SanitizeProperties();
			Metrics.SanitizeMeasurements();
			Url = Url.SanitizeUri();
		}
	}
}
