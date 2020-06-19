namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// An interface implemented by asset provider to offer asset correlate id on-demand.
	/// </summary>
	public interface IAssetProvider
	{
		/// <summary>
		/// Post an asset event for specified asset id with given correlation.
		/// </summary>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <param name="correlation">
		/// The correlation for to-be-posted asset event.
		/// </param>
		/// <returns>
		/// A bool value indicating whether provider posts event successfully.
		/// Return false if input parameters are valid or unexpected error occurs.
		/// </returns>
		/// <remarks>
		/// To create AssetEvent, use constructor <see cref="M:Coding4Fun.VisualStudio.Telemetry.AssetEvent.#ctor(System.String,System.String,System.Int32,Coding4Fun.VisualStudio.Telemetry.TelemetryEventCorrelation)" />
		/// <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetService" /> calls this method on background thread so please make it thread-safe.
		/// </remarks>
		bool PostAsset(string assetId, TelemetryEventCorrelation correlation);
	}
}
