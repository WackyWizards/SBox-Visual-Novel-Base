namespace VNBase.Assets;

/// <summary>
/// A background image asset.
/// </summary>
public class Background : IAsset
{
	public string Path { get; set; }

	public Background( string path )
	{
		Path = path;
	}
}
