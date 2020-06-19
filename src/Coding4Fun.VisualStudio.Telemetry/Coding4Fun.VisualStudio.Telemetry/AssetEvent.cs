using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class that stores information for asset event.
	/// Asset is the target of user task or operation, e.g., Solution, Project, File, Extension, License, Designer.
	/// </summary>
	public sealed class AssetEvent : TelemetryEvent
	{
		private const string AssetEventPropertyPrefixName = "DataModel.Asset.";

		private const string AssetIdPropertyName = "DataModel.Asset.AssetId";

		private const string AssetEventVersionPropertyName = "DataModel.Asset.SchemaVersion";

		/// <summary>
		/// Gets asset id from this asset event.
		/// </summary>
		public string AssetId
		{
			get;
		}

		/// <summary>
		/// Gets the version of this asset event.
		/// </summary>
		public int AssetEventVersion
		{
			get;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" /> class.
		/// </summary>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <param name="assetEventVersion">
		/// Used for customized properties versioning.
		/// E.g., project asset posts event with name "vs/platform/project".
		/// If the event is updated, uses this parameter to increment the version.
		/// </param>
		public AssetEvent(string eventName, string assetId, int assetEventVersion)
			: this(eventName, assetId, assetEventVersion, new TelemetryEventCorrelation(Guid.NewGuid(), DataModelEventType.Asset))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetEvent" /> class.
		/// </summary>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <param name="assetEventVersion">
		/// Used for customized properties versioning.
		/// E.g., project asset posts event with name "vs/platform/project".
		/// If the event is updated, uses this parameter to increment the version.
		/// </param>
		/// <param name="correlation">
		/// Correlation value for this event.
		/// </param>
		public AssetEvent(string eventName, string assetId, int assetEventVersion, TelemetryEventCorrelation correlation)
			: base(eventName, TelemetrySeverity.Normal, correlation)
		{
			if (correlation.EventType != DataModelEventType.Asset)
			{
				throw new ArgumentException("Property EventType should be AssetEvent.", "correlation");
			}
			DataModelEventNameHelper.SetProductFeatureEntityName(this);
			AssetId = assetId;
			base.ReservedProperties["DataModel.Asset.AssetId"] = assetId;
			AssetEventVersion = assetEventVersion;
			base.ReservedProperties["DataModel.Asset.SchemaVersion"] = assetEventVersion;
		}
	}
}
