using Sandbox;

namespace VNBase.Assets;

/// <summary>
/// A background image asset from a dialog.
/// </summary>
public class BackgroundAsset : Asset, Asset.IAsset
{
	/// <summary>
	/// Path to the background image asset.
	/// </summary>
	public string Path { get; set; }
}
