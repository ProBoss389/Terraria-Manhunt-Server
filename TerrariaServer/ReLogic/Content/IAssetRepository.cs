using System;
using System.Collections.Generic;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public interface IAssetRepository : IDisposable
{
	int PendingAssets { get; }

	int TotalAssets { get; }

	int LoadedAssets { get; }

	FailedToLoadAssetCustomAction AssetLoadFailHandler { get; set; }

	void SetSources(IEnumerable<IContentSource> sources, AssetRequestMode mode = AssetRequestMode.ImmediateLoad);

	Asset<T> Request<T>(string assetName, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) where T : class;

	void TransferCompletedAssets();
}
