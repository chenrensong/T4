using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// The class that represents information about performance counters.
	/// </summary>
	[DebuggerDisplay("CategoryName={CategoryName}; CounterName={CounterName}; InstanceName={InstanceName}; Value={Value}; Timestamp={Timestamp}")]
	internal class PerformanceCounterTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "PerformanceCounter";

		internal readonly string BaseType = typeof(PerformanceCounterData).Name;

		internal readonly PerformanceCounterData Data;

		private TelemetryContext context;

		/// <summary>
		/// Gets or sets date and time when telemetry was recorded.
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
		public TelemetryContext Context => LazyInitializer.EnsureInitialized(ref context);

		/// <summary>
		/// Gets or sets the counter value.
		/// </summary>
		public double Value
		{
			get
			{
				return Data.value;
			}
			set
			{
				Data.value = value;
			}
		}

		/// <summary>
		/// Gets or sets the category name.
		/// </summary>
		public string CategoryName
		{
			get
			{
				return Data.categoryName;
			}
			set
			{
				Data.categoryName = value;
			}
		}

		/// <summary>
		/// Gets or sets the counter name.
		/// </summary>
		public string CounterName
		{
			get
			{
				return Data.counterName;
			}
			set
			{
				Data.counterName = value;
			}
		}

		/// <summary>
		/// Gets or sets the instance name.
		/// </summary>
		public string InstanceName
		{
			get
			{
				return Data.instanceName;
			}
			set
			{
				Data.instanceName = value;
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this exception.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		public PerformanceCounterTelemetry()
		{
			Data = new PerformanceCounterData();
		}

		public PerformanceCounterTelemetry(string categoryName, string counterName, string instanceName, double value)
			: this()
		{
			CategoryName = categoryName;
			CounterName = counterName;
			InstanceName = instanceName;
			Value = value;
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
		}
	}
}
