namespace VNBase.Assets;

public class Image( string path ) : IAsset
{
	public string Path { get; set; } = path;
}
