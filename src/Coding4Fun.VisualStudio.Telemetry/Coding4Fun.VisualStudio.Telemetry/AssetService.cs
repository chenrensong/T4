using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class to provide help methods for both asset consumer and providers.
	/// Consumers can use this class to get correlation via method GetCorrelation.
	/// Providers can register existing correlation in this service via method RegisterCorrelation,
	/// or(and) register themselves via method RegisterProvider to send asset events and return correlation per consumers' request.
	/// </summary>
	/// <remarks>
	/// 1. All methods in this service converts Guid value to D-format string (https://msdn.microsoft.com/en-us/library/97af8hh4(v=vs.110).aspx)
	/// 2. This class is thread-safe.
	/// </remarks>
	public sealed class AssetService
	{
		private class BackgroundThreadScheduler : IAssetServiceThreadScheduler
		{
			public void Schedule(Action action)
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					action();
				});
			}
		}

		private struct CacheKey
		{
			private string assetTypeName;

			private string assetId;

			public CacheKey(string assetTypeName, string assetId)
			{
				this.assetTypeName = assetTypeName;
				this.assetId = assetId;
			}
		}

		private object locker = new object();

		private ConcurrentDictionary<string, IAssetProvider> registeredProviders = new ConcurrentDictionary<string, IAssetProvider>();

		private ConcurrentDictionary<CacheKey, TelemetryEventCorrelation> registeredCorrelations = new ConcurrentDictionary<CacheKey, TelemetryEventCorrelation>();

		private static readonly Lazy<AssetService> lazyAssetService = new Lazy<AssetService>(() => new AssetService(new BackgroundThreadScheduler()));

		private IAssetServiceThreadScheduler ThreadScheduler
		{
			get;
		}

		/// <summary>
		/// Gets singleton instance of <see cref="T:Coding4Fun.VisualStudio.Telemetry.AssetService" />
		/// </summary>
		public static AssetService Instance => lazyAssetService.Value;

		internal AssetService(IAssetServiceThreadScheduler scheduler)
		{
			ThreadScheduler = scheduler;
		}

		/// <summary>
		/// Register correlation from a given asset id with specified asset type.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <param name="correlation">correlation of the asset.</param>
		/// <remarks>
		/// Used by Asset Provider.
		/// </remarks>
		public void RegisterCorrelation(string assetTypeName, Guid assetId, TelemetryEventCorrelation correlation)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, assetTypeName);
			CodeContract.RequiresArgumentNotEmpty(assetId, "assetId");
			RegisterCorrelation(assetTypeName, assetId.ToString("D"), correlation);
		}

		/// <summary>
		/// Register correlation from a given asset id with specified asset type.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <param name="correlation">correlation of the asset.</param>
		/// <remarks>
		/// Used by Asset Provider.
		/// </remarks>
		public void RegisterCorrelation(string assetTypeName, string assetId, TelemetryEventCorrelation correlation)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, "assetTypeName");
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetId, "assetId");
			CacheKey key = new CacheKey(assetTypeName, assetId);
			registeredCorrelations[key] = correlation;
		}

		/// <summary>
		/// Unregister correlation from this service.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <remarks>
		/// Used by Asset Provider.
		/// Call this method when previous registered asset correlation is stale.
		/// </remarks>
		public void UnregisterCorrelation(string assetTypeName, Guid assetId)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, "assetTypeName");
			CodeContract.RequiresArgumentNotEmpty(assetId, "assetId");
			UnregisterCorrelation(assetTypeName, assetId.ToString("D"));
		}

		/// <summary>
		/// Unregister correlation from this service.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// </param>
		/// <remarks>
		/// Used by Asset Provider.
		/// Call this method when previous registered asset correlation is stale.
		/// </remarks>
		public void UnregisterCorrelation(string assetTypeName, string assetId)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, "assetTypeName");
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetId, "assetId");
			CacheKey key = new CacheKey(assetTypeName, assetId);
			registeredCorrelations.TryRemove(key, out TelemetryEventCorrelation _);
		}

		/// <summary>
		/// Register asset provider which can send asset event and return correlation per consumers' request.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		/// <param name="assetProvider">Asset provider</param>
		public void RegisterProvider(string assetTypeName, IAssetProvider assetProvider)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, "assetTypeName");
			CodeContract.RequiresArgumentNotNull<IAssetProvider>(assetProvider, "assetProvider");
			registeredProviders[assetTypeName] = assetProvider;
		}

		/// <summary>
		/// Unregister asset provider.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		public void UnregisterProvider(string assetTypeName)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, "assetTypeName");
			registeredProviders.TryRemove(assetTypeName, out IAssetProvider _);
		}

		/// <summary>
		/// Get correlation for a given asset type and asset id.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// You can get more information from asset provider.
		/// </param>
		/// <returns>Asset correlation</returns>
		public TelemetryEventCorrelation GetCorrelation(string assetTypeName, Guid assetId)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, "assetTypeName");
			CodeContract.RequiresArgumentNotEmpty(assetId, "assetId");
			return GetCorrelation(assetTypeName, assetId.ToString("D"));
		}

		/// <summary>
		/// Get correlation for a given asset type and asset id.
		/// </summary>
		/// <param name="assetTypeName">Asset type name. It is defined by asset provider.</param>
		/// <param name="assetId">
		/// Used to identify the asset. The id should be immutable in the asset life cycle, even if the status or content changes over time.
		/// E.g., project guid is generated during project creation and will never change. This makes it a good candidate for asset id of Project asset.
		/// You can get more information from asset provider.
		/// </param>
		/// <returns>Asset correlation</returns>
		public TelemetryEventCorrelation GetCorrelation(string assetTypeName, string assetId)
		{
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetTypeName, "assetTypeName");
			CodeContract.RequiresArgumentNotNullAndNotWhiteSpace(assetId, "assetId");
			IAssetProvider provider = null;
			CacheKey key = new CacheKey(assetTypeName, assetId);
			TelemetryEventCorrelation correlation = TelemetryEventCorrelation.Empty;
			lock (locker)
			{
				if (!registeredCorrelations.TryGetValue(key, out correlation))
				{
					correlation = TelemetryEventCorrelation.Empty;
					if (registeredProviders.TryGetValue(assetTypeName, out provider))
					{
						correlation = new TelemetryEventCorrelation(Guid.NewGuid(), DataModelEventType.Asset);
						RegisterCorrelation(assetTypeName, assetId, correlation);
					}
				}
			}
			if (provider != null)
			{
				ThreadScheduler.Schedule(delegate
				{
					bool flag = false;
					try
					{
						flag = provider.PostAsset(assetId, correlation);
					}
					finally
					{
						if (!flag)
						{
							UnregisterCorrelation(assetTypeName, assetId);
						}
					}
				});
			}
			return correlation;
		}
	}
}
