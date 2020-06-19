using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights
{
	/// <summary>
	/// Send events, metrics and other telemetry to the Application Insights service.
	/// </summary>
	public sealed class TelemetryClient
	{
		private readonly TelemetryConfiguration configuration;

		private TelemetryContext context;

		private ITelemetryChannel channel;

		/// <summary>
		/// Gets the current context that will be used to augment telemetry you send.
		/// </summary>
		public TelemetryContext Context
		{
			get
			{
				return LazyInitializer.EnsureInitialized(ref context, CreateInitializedContext);
			}
			internal set
			{
				context = value;
			}
		}

		/// <summary>
		/// Gets or sets the default instrumentation key for all <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> objects logged in this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" />.
		/// </summary>
		public string InstrumentationKey
		{
			get
			{
				return Context.InstrumentationKey;
			}
			set
			{
				Context.InstrumentationKey = value;
			}
		}

		/// <summary>
		/// Gets or sets the channel used by the client helper. Note that this doesn't need to be public as a customer can create a new client
		/// with a new channel via telemetry configuration.
		/// </summary>
		internal ITelemetryChannel Channel
		{
			get
			{
				ITelemetryChannel telemetryChannel = channel;
				if (telemetryChannel == null)
				{
					telemetryChannel = (channel = configuration.TelemetryChannel);
				}
				return telemetryChannel;
			}
			set
			{
				channel = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" /> class. Send telemetry with the active configuration, usually loaded from ApplicationInsights.config.
		/// </summary>
		public TelemetryClient()
			: this(TelemetryConfiguration.Active)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" /> class. Send telemetry with the specified <paramref name="configuration" />.
		/// </summary>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="configuration" /> is null.</exception>
		public TelemetryClient(TelemetryConfiguration configuration)
		{
			if (configuration == null)
			{
				CoreEventSource.Log.TelemetryClientConstructorWithNoTelemetryConfiguration();
				configuration = TelemetryConfiguration.Active;
			}
			this.configuration = configuration;
		}

		/// <summary>
		/// Check to determine if the tracking is enabled.
		/// </summary>
		/// <returns></returns>
		public bool IsEnabled()
		{
			return !configuration.DisableTelemetry;
		}

		/// <summary>
		/// Send an <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.EventTelemetry" /> for display in Diagnostic Search and aggregation in Metrics Explorer.
		/// </summary>
		/// <param name="eventName">A name for the event.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		/// <param name="metrics">Measurements associated with this event.</param>
		public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
		{
			EventTelemetry eventTelemetry = new EventTelemetry(eventName);
			if (properties != null && properties.Count > 0)
			{
				Utils.CopyDictionary(properties, eventTelemetry.Context.Properties);
			}
			if (metrics != null && metrics.Count > 0)
			{
				Utils.CopyDictionary(metrics, eventTelemetry.Metrics);
			}
			TrackEvent(eventTelemetry);
		}

		/// <summary>
		/// Send an <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.EventTelemetry" /> for display in Diagnostic Search and aggregation in Metrics Explorer.
		/// </summary>
		/// <param name="telemetry">An event log item.</param>
		public void TrackEvent(EventTelemetry telemetry)
		{
			if (telemetry == null)
			{
				telemetry = new EventTelemetry();
			}
			Track(telemetry);
		}

		/// <summary>
		/// Send a trace message for display in Diagnostic Search.
		/// </summary>
		/// <param name="message">Message to display.</param>
		public void TrackTrace(string message)
		{
			TrackTrace(new TraceTelemetry(message));
		}

		/// <summary>
		/// Send a trace message for display in Diagnostic Search.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="severityLevel">Trace severity level.</param>
		public void TrackTrace(string message, SeverityLevel severityLevel)
		{
			TrackTrace(new TraceTelemetry(message, severityLevel));
		}

		/// <summary>
		/// Send a trace message for display in Diagnostic Search.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		public void TrackTrace(string message, IDictionary<string, string> properties)
		{
			TraceTelemetry traceTelemetry = new TraceTelemetry(message);
			if (properties != null && properties.Count > 0)
			{
				Utils.CopyDictionary(properties, traceTelemetry.Context.Properties);
			}
			TrackTrace(traceTelemetry);
		}

		/// <summary>
		/// Send a trace message for display in Diagnostic Search.
		/// </summary>
		/// <param name="message">Message to display.</param>
		/// <param name="severityLevel">Trace severity level.</param>
		/// <param name="properties">Named string values you can use to search and classify events.</param>
		public void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
		{
			TraceTelemetry traceTelemetry = new TraceTelemetry(message, severityLevel);
			if (properties != null && properties.Count > 0)
			{
				Utils.CopyDictionary(properties, traceTelemetry.Context.Properties);
			}
			TrackTrace(traceTelemetry);
		}

		/// <summary>
		/// Send a trace message for display in Diagnostic Search.
		/// </summary>
		/// <param name="telemetry">Message with optional properties.</param>
		public void TrackTrace(TraceTelemetry telemetry)
		{
			telemetry = (telemetry ?? new TraceTelemetry());
			Track(telemetry);
		}

		/// <summary>
		/// Send a <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.MetricTelemetry" /> for aggregation in Metric Explorer.
		/// </summary>
		/// <param name="name">Metric name.</param>
		/// <param name="value">Metric value.</param>
		/// <param name="properties">Named string values you can use to classify and filter metrics.</param>
		public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
		{
			MetricTelemetry metricTelemetry = new MetricTelemetry(name, value);
			if (properties != null && properties.Count > 0)
			{
				Utils.CopyDictionary(properties, metricTelemetry.Properties);
			}
			TrackMetric(metricTelemetry);
		}

		/// <summary>
		/// Send a <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.MetricTelemetry" /> for aggregation in Metric Explorer.
		/// </summary>
		public void TrackMetric(MetricTelemetry telemetry)
		{
			if (telemetry == null)
			{
				telemetry = new MetricTelemetry();
			}
			Track(telemetry);
		}

		/// <summary>
		/// Send an <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.ExceptionTelemetry" /> for display in Diagnostic Search.
		/// </summary>
		/// <param name="exception">The exception to log.</param>
		/// <param name="properties">Named string values you can use to classify and search for this exception.</param>
		/// <param name="metrics">Additional values associated with this exception.</param>
		public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
		{
			if (exception == null)
			{
				exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
			}
			ExceptionTelemetry exceptionTelemetry = new ExceptionTelemetry(exception)
			{
				HandledAt = ExceptionHandledAt.UserCode
			};
			if (properties != null && properties.Count > 0)
			{
				Utils.CopyDictionary(properties, exceptionTelemetry.Context.Properties);
			}
			if (metrics != null && metrics.Count > 0)
			{
				Utils.CopyDictionary(metrics, exceptionTelemetry.Metrics);
			}
			TrackException(exceptionTelemetry);
		}

		/// <summary>
		/// Send an <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.ExceptionTelemetry" /> for display in Diagnostic Search.
		/// </summary>
		public void TrackException(ExceptionTelemetry telemetry)
		{
			if (telemetry == null)
			{
				telemetry = new ExceptionTelemetry(new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName)))
				{
					HandledAt = ExceptionHandledAt.UserCode
				};
			}
			Track(telemetry);
		}

		/// <summary>
		/// This method is an internal part of Application Insights infrastructure. Do not call.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Track(ITelemetry telemetry)
		{
			if (!IsEnabled())
			{
				return;
			}
			string instrumentationKey = Context.InstrumentationKey;
			if (string.IsNullOrEmpty(instrumentationKey))
			{
				instrumentationKey = configuration.InstrumentationKey;
			}
			if (string.IsNullOrEmpty(instrumentationKey))
			{
				return;
			}
			ISupportProperties supportProperties = telemetry as ISupportProperties;
			if (supportProperties != null)
			{
				if (Channel.DeveloperMode)
				{
					supportProperties.Properties.Add("DeveloperMode", "true");
				}
				Utils.CopyDictionary(Context.Properties, supportProperties.Properties);
			}
			telemetry.Context.Initialize(Context, instrumentationKey);
			foreach (ITelemetryInitializer telemetryInitializer in configuration.TelemetryInitializers)
			{
				try
				{
					telemetryInitializer.Initialize(telemetry);
				}
				catch (Exception ex)
				{
					CoreEventSource.Log.LogError(string.Format(CultureInfo.InvariantCulture, "Exception while initializing {0}, exception message - {1}", new object[2]
					{
						telemetryInitializer.GetType().FullName,
						ex.ToString()
					}));
				}
			}
			telemetry.Sanitize();
			Channel.Send(telemetry);
		}

		/// <summary>
		/// Send information about the page viewed in the application.
		/// </summary>
		/// <param name="name">Name of the page.</param>
		public void TrackPageView(string name)
		{
			Track(new PageViewTelemetry(name));
		}

		/// <summary>
		/// Send information about the page viewed in the application.
		/// </summary>
		public void TrackPageView(PageViewTelemetry telemetry)
		{
			if (telemetry == null)
			{
				telemetry = new PageViewTelemetry();
			}
			Track(telemetry);
		}

		/// <summary>
		/// Send information about a request handled by the application.
		/// </summary>
		/// <param name="name">The request name.</param>
		/// <param name="timestamp">The time when the page was requested.</param>
		/// <param name="duration">The time taken by the application to handle the request.</param>
		/// <param name="responseCode">The response status code.</param>
		/// <param name="success">True if the request was handled successfully by the application.</param>
		public void TrackRequest(string name, DateTimeOffset timestamp, TimeSpan duration, string responseCode, bool success)
		{
			Track(new RequestTelemetry(name, timestamp, duration, responseCode, success));
		}

		/// <summary>
		/// Send information about a request handled by the application.
		/// </summary>
		public void TrackRequest(RequestTelemetry request)
		{
			Track(request);
		}

		/// <summary>
		/// Flushes channel.
		/// </summary>
		public void Flush()
		{
			Channel.Flush();
		}

		/// <summary>
		/// Flushes channel and wait until transmit is fully completed. Wait can be cancelled by using CancellationToken.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Waitable and cancellable task</returns>
		public async Task FlushAndTransmitAsync(CancellationToken token)
		{
			await Channel.FlushAndTransmitAsync(token).ConfigureAwait(false);
		}

		private TelemetryContext CreateInitializedContext()
		{
			TelemetryContext result = new TelemetryContext();
			foreach (IContextInitializer contextInitializer in configuration.ContextInitializers)
			{
				contextInitializer.Initialize(result);
			}
			return result;
		}
	}
}
