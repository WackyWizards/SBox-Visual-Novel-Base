using Sandbox;

namespace VNBase;

/// <summary>
/// A background image asset from a dialog.
/// </summary>
public class BackgroundAsset : AssetBase, AssetBase.IAsset
{
	/// <summary>
	/// Path to the background image asset.
	/// </summary>
	public string Path { get; set; }
}
