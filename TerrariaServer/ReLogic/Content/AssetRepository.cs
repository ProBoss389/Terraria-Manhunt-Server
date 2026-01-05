using System;
using System.Collections.Generic;
using System.Linq;
using ReLogic.Content.Sources;

namespace ReLogic.Content;

public class AssetRepository : IAssetRepository, IDisposable
{
	private readonly Dictionary<string, IAsset> _assets = new Dictionary<string, IAsset>();
	private IEnumerable<IContentSource> _sources = new IContentSource[0];
	private readonly Dictionary<Type, Action<IAsset, AssetRequestMode>> _typeSpecificReloadActions = new Dictionary<Type, Action<IAsset, AssetRequestMode>>();
	private readonly IAsyncAssetLoader _asyncLoader;
	private readonly IAssetLoader _loader;
	private readonly object _requestLock = new object();
	private bool _isDisposed;

	public int PendingAssets => _asyncLoader.Remaining;

	public int TotalAssets { get; private set; }

	public int LoadedAssets { get; private set; }

	public bool IsAsyncLoadingEnabled => _asyncLoader.IsRunning;

	public FailedToLoadAssetCustomAction AssetLoadFailHandler { get; set; }

	public AssetRepository(IAssetLoader syncLoader, IAsyncAssetLoader asyncLoader)
	{
		_loader = syncLoader;
		_asyncLoader = asyncLoader;
		_asyncLoader.Start();
	}

	internal AssetRepository(IAssetLoader syncLoader, IAsyncAssetLoader asyncLoader, bool useAsync)
	{
		_loader = syncLoader;
		_asyncLoader = asyncLoader;
		if (useAsync)
			_asyncLoader.Start();
	}

	public void SetSources(IEnumerable<IContentSource> sources, AssetRequestMode mode = AssetRequestMode.ImmediateLoad)
	{
		ThrowIfDisposed();
		lock (_requestLock) {
			while (_asyncLoader.Remaining > 0) {
				_asyncLoader.TransferCompleted();
			}

			_sources = sources.ToList();
			ReloadAssetsIfSourceChanged(mode);
		}
	}

	public Asset<T> Request<T>(string assetName, AssetRequestMode mode = AssetRequestMode.ImmediateLoad) where T : class
	{
		ThrowIfDisposed();
		assetName = AssetPathHelper.CleanPath(assetName);
		lock (_requestLock) {
			Asset<T> asset = null;
			if (_assets.TryGetValue(assetName, out var value))
				asset = value as Asset<T>;

			if (asset == null) {
				asset = new Asset<T>(assetName);
				_assets[assetName] = asset;
			}
			else if (asset.State != 0) {
				return asset;
			}

			LoadAsset(asset, mode);
			return asset;
		}
	}

	public void TransferCompletedAssets()
	{
		ThrowIfDisposed();
		lock (_requestLock) {
			_asyncLoader.TransferCompleted();
		}
	}

	private void ReloadAssetsIfSourceChanged(AssetRequestMode mode)
	{
		foreach (IAsset item in _assets.Values.Where((IAsset asset) => asset.IsLoaded)) {
			IContentSource contentSource = FindSourceForAsset(item.Name);
			if (contentSource == null)
				ForceReloadAsset(item, AssetRequestMode.DoNotLoad);
			else if (item.Source != contentSource)
				ForceReloadAsset(item, (item.State != 0) ? mode : AssetRequestMode.DoNotLoad);
		}
	}

	private void LoadAsset<T>(Asset<T> asset, AssetRequestMode mode) where T : class
	{
		if (mode == AssetRequestMode.DoNotLoad)
			return;

		EnsureReloadActionExistsForType<T>();
		TotalAssets++;
		asset.SetToLoadingState();
		try {
			TryLoadingAsset(asset, mode);
		}
		catch (Exception e) {
			if (AssetLoadFailHandler != null)
				AssetLoadFailHandler(asset.Name, e);

			throw;
		}
	}

	private void TryLoadingAsset<T>(Asset<T> asset, AssetRequestMode mode) where T : class
	{
		IContentSource source = FindSourceForAsset(asset.Name);
		switch (mode) {
			case AssetRequestMode.ImmediateLoad: {
				bool flag = true;
				IRejectionReason rejectionReason = null;
				if (!_loader.TryLoad<T>(asset.Name, source, out var resultAsset)) {
					source.RejectAsset(asset.Name, new ContentRejectionFromFailedLoad_ByAssetExtensionType());
					TryLoadingAsset(asset, mode);
					break;
				}

				if (source.ContentValidator != null)
					flag = source.ContentValidator.AssetIsValid(resultAsset, asset.Name, out rejectionReason);

				if (flag) {
					asset.SubmitLoadedContent(resultAsset, source);
					LoadedAssets++;
				}
				else {
					source.RejectAsset(asset.Name, rejectionReason);
					TryLoadingAsset(asset, mode);
				}

				break;
			}
			case AssetRequestMode.AsyncLoad:
				_asyncLoader.Load(asset.Name, source, delegate (bool proper, T content) {
					if (!proper) {
						source.RejectAsset(asset.Name, new ContentRejectionFromFailedLoad_ByAssetExtensionType());
						TryLoadingAsset(asset, mode);
					}
					else {
						bool flag2 = true;
						IRejectionReason rejectionReason2 = null;
						if (source.ContentValidator != null)
							flag2 = source.ContentValidator.AssetIsValid(content, asset.Name, out rejectionReason2);

						if (flag2) {
							asset.SubmitLoadedContent(content, source);
							int loadedAssets = LoadedAssets;
							LoadedAssets = loadedAssets + 1;
						}
						else {
							source.RejectAsset(asset.Name, rejectionReason2);
							TryLoadingAsset(asset, mode);
						}
					}
				});
				break;
			default:
				throw new ArgumentOutOfRangeException("mode", mode, null);
		}
	}

	private void ForceReloadAsset(IAsset asset, AssetRequestMode mode)
	{
		if (mode != 0)
			LoadedAssets--;

		_typeSpecificReloadActions[asset.GetType()](asset, mode);
	}

	private void EnsureReloadActionExistsForType<T>() where T : class
	{
		_typeSpecificReloadActions[typeof(Asset<T>)] = ForceReloadAsset<T>;
	}

	private void ForceReloadAsset<T>(IAsset asset, AssetRequestMode mode) where T : class
	{
		Asset<T> asset2 = (Asset<T>)asset;
		asset2.Unload();
		LoadAsset(asset2, mode);
	}

	private IContentSource FindSourceForAsset(string assetName) => _sources.FirstOrDefault((IContentSource source) => source.HasAsset(assetName)) ?? throw AssetLoadException.FromMissingAsset(assetName);

	private void ThrowIfDisposed()
	{
		if (_isDisposed)
			throw new ObjectDisposedException("AssetRepository is disposed.");
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed)
			return;

		if (disposing) {
			_asyncLoader.Dispose();
			foreach (KeyValuePair<string, IAsset> asset in _assets) {
				asset.Value.Dispose();
			}
		}

		_isDisposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
