using ReLogic.Content.Sources;

namespace ReLogic.Content;

public class AssetLoader : IAssetLoader
{
	private readonly AssetReaderCollection _readers;

	public AssetLoader(AssetReaderCollection readers)
	{
		_readers = readers;
	}

	public bool TryLoad<T>(string assetName, IContentSource source, out T resultAsset) where T : class
	{
		resultAsset = null;
		string extension = source.GetExtension(assetName);
		if (!_readers.CanReadExtension(extension))
			return false;

		resultAsset = _readers.Read<T>(source.OpenStream(assetName), extension);
		return true;
	}
}
