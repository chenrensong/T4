using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Represents a context for sending telemetry to the Application Insights service.
	/// </summary>
	public sealed class TelemetryContext : IJsonSerializable
	{
		private readonly IDictionary<string, string> properties;

		private readonly IDictionary<string, string> tags;

		private string instrumentationKey;

		private ComponentContext component;

		private DeviceContext device;

		private SessionContext session;

		private UserContext user;

		private OperationContext operation;

		private LocationContext location;

		private InternalContext internalContext;

		/// <summary>
		/// Gets or sets the default instrumentation key for all <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> objects logged in this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		/// <remarks>
		/// By default, this property is initialized with the <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration.InstrumentationKey" /> value
		/// of the <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration.Active" /> instance of <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration" />. You can specify it
		/// for all telemetry tracked via a particular <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.TelemetryClient" /> or for a specific <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" />
		/// instance.
		/// </remarks>
		public string InstrumentationKey
		{
			get
			{
				return instrumentationKey ?? string.Empty;
			}
			set
			{
				Property.Set(instrumentationKey, value);
			}
		}

		/// <summary>
		/// Gets the object describing the component tracked by this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		public ComponentContext Component => LazyInitializer.EnsureInitialized(ref component, () => new ComponentContext(Tags));

		/// <summary>
		/// Gets the object describing the device tracked by this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		public DeviceContext Device => LazyInitializer.EnsureInitialized(ref device, () => new DeviceContext(Tags));

		/// <summary>
		/// Gets the object describing a user session tracked by this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		public SessionContext Session => LazyInitializer.EnsureInitialized(ref session, () => new SessionContext(Tags));

		/// <summary>
		/// Gets the object describing a user tracked by this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		public UserContext User => LazyInitializer.EnsureInitialized(ref user, () => new UserContext(Tags));

		/// <summary>
		/// Gets the object describing a operation tracked by this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		public OperationContext Operation => LazyInitializer.EnsureInitialized(ref operation, () => new OperationContext(Tags));

		/// <summary>
		/// Gets the object describing a location tracked by this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		public LocationContext Location => LazyInitializer.EnsureInitialized(ref location, () => new LocationContext(Tags));

		/// <summary>
		/// Gets a dictionary of application-defined property values.
		/// </summary>
		public IDictionary<string, string> Properties => properties;

		internal InternalContext Internal => LazyInitializer.EnsureInitialized(ref internalContext, () => new InternalContext(Tags));

		/// <summary>
		/// Gets a dictionary of context tags.
		/// </summary>
		internal IDictionary<string, string> Tags => tags;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" /> class.
		/// </summary>
		public TelemetryContext()
			: this(new SnapshottingDictionary<string, string>(), new SnapshottingDictionary<string, string>())
		{
		}

		internal TelemetryContext(IDictionary<string, string> properties, IDictionary<string, string> tags)
		{
			this.properties = properties;
			this.tags = tags;
		}

		/// <summary>
		/// Serializes this object in JSON format.
		/// </summary>
		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteProperty("iKey", InstrumentationKey);
			writer.WriteProperty("tags", Tags);
		}

		internal void Initialize(TelemetryContext source, string instrumentationKey)
		{
			Property.Initialize(this.instrumentationKey, instrumentationKey);
			if (source.tags != null && source.tags.Count > 0)
			{
				Utils.CopyDictionary(source.tags, Tags);
			}
		}
	}
}
