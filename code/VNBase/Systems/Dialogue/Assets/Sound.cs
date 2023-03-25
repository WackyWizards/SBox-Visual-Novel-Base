using Sandbox;

namespace VNBase;

/// <summary>
/// A playable sound asset from a dialog.
/// </summary>
public class SoundAsset : AssetBase, AssetBase.IAsset
{
	/// <summary>
	/// Path to the sound asset.
	/// </summary>
	public string Path { get; set; }
}
