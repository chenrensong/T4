using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class is to wrap up settings used to control TelemetryScope behavior when create an instance of it.
	/// </summary>
	public sealed class TelemetryScopeSettings
	{
		/// <summary>
		/// Gets or sets event properties which are added to both start and end events when create an instance of TelemetryScope.
		/// </summary>
		public IDictionary<string, object> StartEventProperties
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets event severity for both start and end events in TelemetryScope.
		/// </summary>
		public TelemetrySeverity Severity
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether events are opted out friendly or not.
		/// </summary>
		public bool IsOptOutFriendly
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets correlations for both start and end events in TelemetryScope.
		/// </summary>
		public TelemetryEventCorrelation[] Correlations
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether post start event or not when create an instance of TelemetryScope.
		/// </summary>
		public bool PostStartEvent
		{
			get;
			set;
		}

		/// <summary>
		/// Creates the new TelemetryScopeSettings instance.
		/// </summary>
		public TelemetryScopeSettings()
		{
			StartEventProperties = null;
			Severity = TelemetrySeverity.Normal;
			IsOptOutFriendly = false;
			Correlations = null;
			PostStartEvent = true;
		}
	}
}
