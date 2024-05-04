namespace VNBase.Assets;

/// <summary>
/// A background image asset.
/// </summary>
public class BackgroundAsset : Asset, Asset.IAsset
{
	/// <summary>
	/// Path to the background image asset.
	/// </summary>
	public string Path { get; set; } = string.Empty;

	public BackgroundAsset(string path)
	{
		Path = path;
	}
}
