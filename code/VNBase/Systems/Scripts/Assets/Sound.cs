using Sandbox;

namespace VNBase.Assets;

/// <summary>
/// A playable sound asset.
/// </summary>
public class SoundAsset : Asset, Asset.IAsset
{
	/// <summary>
	/// Path to the sound asset.
	/// </summary>
	public string Path { get; set; } = string.Empty;
}
