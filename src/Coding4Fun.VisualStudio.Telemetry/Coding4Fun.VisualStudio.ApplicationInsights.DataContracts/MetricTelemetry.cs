using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Telemetry type used to track metrics.
	/// </summary>
	public sealed class MetricTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "Metric";

		internal readonly string BaseType = typeof(MetricData).Name;

		internal readonly MetricData Data;

		internal readonly DataPoint Metric;

		private readonly TelemetryContext context;

		private bool isAggregation;

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
				return Metric.name;
			}
			set
			{
				Metric.name = value;
			}
		}

		/// <summary>
		/// Gets or sets the value of this metric.
		/// </summary>
		public double Value
		{
			get
			{
				return Metric.value;
			}
			set
			{
				Metric.value = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of samples for this metric.
		/// </summary>
		public int? Count
		{
			get
			{
				return Metric.count;
			}
			set
			{
				Metric.count = value;
				UpdateKind();
			}
		}

		/// <summary>
		/// Gets or sets the min value of this metric.
		/// </summary>
		public double? Min
		{
			get
			{
				return Metric.min;
			}
			set
			{
				Metric.min = value;
				UpdateKind();
			}
		}

		/// <summary>
		/// Gets or sets the max value of this metric.
		/// </summary>
		public double? Max
		{
			get
			{
				return Metric.max;
			}
			set
			{
				Metric.max = value;
				UpdateKind();
			}
		}

		/// <summary>
		/// Gets or sets the standard deviation of this metric.
		/// </summary>
		public double? StandardDeviation
		{
			get
			{
				return Metric.stdDev;
			}
			set
			{
				Metric.stdDev = value;
				UpdateKind();
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this metric.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.MetricTelemetry" /> class with empty
		/// properties.
		/// </summary>
		public MetricTelemetry()
		{
			Data = new MetricData();
			Metric = new DataPoint();
			context = new TelemetryContext(Data.properties, new Dictionary<string, string>());
			Data.metrics.Add(Metric);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.MetricTelemetry" /> class with the
		/// specified <paramref name="metricName" /> and <paramref name="metricValue" />.
		/// </summary>
		/// <exception cref="T:System.ArgumentException">The <paramref name="metricName" /> is null or empty string.</exception>
		public MetricTelemetry(string metricName, double metricValue)
			: this()
		{
			Name = metricName;
			Value = metricValue;
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
			Name = Name.SanitizeName();
			Name = Utils.PopulateRequiredStringValue(Name, "name", typeof(MetricTelemetry).FullName);
			Properties.SanitizeProperties();
		}

		private void UpdateKind()
		{
			bool flag = Metric.count.HasValue || Metric.min.HasValue || Metric.max.HasValue || Metric.stdDev.HasValue;
			if (isAggregation != flag)
			{
				if (flag)
				{
					Metric.kind = DataPointType.Aggregation;
				}
				else
				{
					Metric.kind = DataPointType.Measurement;
				}
			}
			isAggregation = flag;
		}
	}
}
