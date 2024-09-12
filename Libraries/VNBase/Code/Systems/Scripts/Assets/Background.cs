namespace VNBase.Assets;

/// <summary>
/// A background image asset.
/// </summary>
public class BackgroundAsset : IAsset
{
	public string Path { get; set; } = string.Empty;

	public BackgroundAsset( string path )
	{
		Path = path;
	}
}
