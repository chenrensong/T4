using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// Encapsulates the global telemetry configuration typically loaded from the ApplicationInsights.config file.
	/// </summary>
	/// <remarks>
	/// All <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" /> objects are initialized using the <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration.Active" />
	/// telemetry configuration provided by this class.
	/// </remarks>
	public sealed class TelemetryConfiguration : IDisposable
	{
		private static object syncRoot = new object();

		private static TelemetryConfiguration active;

		private readonly SnapshottingList<IContextInitializer> contextInitializers = new SnapshottingList<IContextInitializer>();

		private readonly SnapshottingList<ITelemetryInitializer> telemetryInitializers = new SnapshottingList<ITelemetryInitializer>();

		private readonly SnapshottingList<object> telemetryModules = new SnapshottingList<object>();

		private string instrumentationKey = string.Empty;

		private bool disableTelemetry;

		/// <summary>
		/// Gets the active <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration" /> instance loaded from the ApplicationInsights.config file.
		/// If the configuration file does not exist, the active configuration instance is initialized with minimum defaults
		/// needed to send telemetry to Application Insights.
		/// </summary>
		public static TelemetryConfiguration Active
		{
			get
			{
				if (active == null)
				{
					lock (syncRoot)
					{
						if (active == null)
						{
							active = new TelemetryConfiguration();
							TelemetryConfigurationFactory.Instance.Initialize(active);
						}
					}
				}
				return active;
			}
			internal set
			{
				lock (syncRoot)
				{
					active = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the default instrumentation key for the application.
		/// </summary>
		/// <exception cref="T:System.ArgumentNullException">The new value is null.</exception>
		/// <remarks>
		/// This instrumentation key value is used by default by all <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" /> instances
		/// created in the application. This value can be overwritten by setting the <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext.InstrumentationKey" />
		/// property of the <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient.Context" />.
		/// </remarks>
		public string InstrumentationKey
		{
			get
			{
				return instrumentationKey;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				instrumentationKey = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether sending of telemetry to Application Insights is disabled.
		/// </summary>
		/// <remarks>
		/// This disable tracking setting value is used by default by all <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" /> instances
		/// created in the application.
		/// </remarks>
		public bool DisableTelemetry
		{
			get
			{
				return disableTelemetry;
			}
			set
			{
				if (value)
				{
					CoreEventSource.Log.TrackingWasDisabled();
				}
				else
				{
					CoreEventSource.Log.TrackingWasEnabled();
				}
				disableTelemetry = value;
			}
		}

		/// <summary>
		/// Gets the list of <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.IContextInitializer" /> objects that supply additional information about application.
		/// </summary>
		/// <remarks>
		/// Context initializers extend Application Insights telemetry collection by supplying additional information
		/// about application environment, such as <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext.User" /> or <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext.Device" />
		/// information that remains constant during application lifetime. A <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" /> invokes context
		/// initializers to obtain initial property values for <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" /> object during its construction.
		/// The default list of context initializers is provided by the Application Insights NuGet packages and loaded from
		/// the ApplicationInsights.config file located in the application directory.
		/// </remarks>
		public IList<IContextInitializer> ContextInitializers => contextInitializers;

		/// <summary>
		/// Gets the list of <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.ITelemetryInitializer" /> objects that supply additional information about telemetry.
		/// </summary>
		/// <remarks>
		/// Telemetry initializers extend Application Insights telemetry collection by supplying additional information
		/// about individual <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> items, such as <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry.Timestamp" />. A <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" />
		/// invokes telemetry initializers each time <see cref="M:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient.Track(Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry)" /> method is called.
		/// The default list of telemetry initializers is provided by the Application Insights NuGet packages and loaded from
		/// the ApplicationInsights.config file located in the application directory.
		/// </remarks>
		public IList<ITelemetryInitializer> TelemetryInitializers => telemetryInitializers;

		/// <summary>
		/// Gets the list of modules that automatically generate application telemetry.
		/// </summary>
		/// <remarks>
		/// Telemetry modules automatically send telemetry describing the application to Application Insights. For example, a telemetry
		/// module can handle application exception events and automatically send <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.ExceptionTelemetry" /> you can see on the
		/// Application Insights portal.
		/// The default list of telemetry modules is provided by the Application Insights NuGet packages and loaded from
		/// the ApplicationInsights.config file located in the application directory.
		/// </remarks>
		public IList<object> TelemetryModules => telemetryModules;

		/// <summary>
		/// Gets or sets the telemetry channel.
		/// </summary>
		public ITelemetryChannel TelemetryChannel
		{
			get;
			set;
		}

		/// <summary>
		/// Creates a new <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration" /> instance loaded from the ApplicationInsights.config file.
		/// If the configuration file does not exist, the new configuration instance is initialized with minimum defaults
		/// needed to send telemetry to Application Insights.
		/// </summary>
		/// <returns></returns>
		public static TelemetryConfiguration CreateDefault()
		{
			TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration();
			TelemetryConfigurationFactory.Instance.Initialize(telemetryConfiguration);
			return telemetryConfiguration;
		}

		/// <summary>
		/// Releases resources used by the current instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration" /> class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				Interlocked.CompareExchange(ref active, null, this);
				TelemetryChannel?.Dispose();
			}
		}
	}
}
