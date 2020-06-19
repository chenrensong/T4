using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// The class that represents information about collected RDD.
	/// </summary>
	[DebuggerDisplay("Value={Value}; Name={Name}; Count={Count}; Success={Success}; Async={Async}; Timestamp={Timestamp}")]
	internal sealed class RemoteDependencyTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "RemoteDependency";

		internal readonly string BaseType = typeof(RemoteDependencyData).Name;

		internal readonly RemoteDependencyData Data;

		private readonly TelemetryContext context;

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
		public TelemetryContext Context => context;

		/// <summary>
		/// Gets or sets resource name.
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
		/// Gets or sets text of SQL command or empty it not applicable.
		/// </summary>
		public string CommandName
		{
			get
			{
				return Data.commandName;
			}
			set
			{
				Data.commandName = value;
			}
		}

		/// <summary>
		/// Gets or sets the dependency kind, like SQL, HTTP, Azure, etc.
		/// </summary>
		public DependencyKind DependencyKind
		{
			get
			{
				return Data.dependencyKind;
			}
			set
			{
				Data.dependencyKind = value;
			}
		}

		/// <summary>
		/// Gets or sets request duration.
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
		/// Gets or sets request count.
		/// </summary>
		public int? Count
		{
			get
			{
				return Data.count;
			}
			set
			{
				Data.count = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the dependency call was successful or not.
		/// </summary>
		public bool? Success
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
		/// Gets or sets a value indicating whether the dependency call was made asynchronously or not.
		/// </summary>
		public bool? Async
		{
			get
			{
				return Data.async;
			}
			set
			{
				Data.async = value;
			}
		}

		/// <summary>
		/// Gets or sets a value identifying the source of RDD.
		/// </summary>
		public DependencySourceType DependencySource
		{
			get
			{
				return Data.dependencySource;
			}
			set
			{
				Data.dependencySource = value;
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this remote dependency.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		public RemoteDependencyTelemetry()
		{
			Data = new RemoteDependencyData
			{
				kind = DataPointType.Aggregation
			};
			context = new TelemetryContext(Data.properties, new Dictionary<string, string>());
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
			Name = Name.SanitizeName();
			Properties.SanitizeProperties();
		}
	}
}
