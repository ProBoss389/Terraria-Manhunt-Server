namespace ReLogic.Content;

public class ContentRejectionFromFailedLoad_ByAssetExtensionType : IRejectionReason
{
	public string GetReason() => "Only textures of type '.png' and '.xnb' may be loaded.";
}
