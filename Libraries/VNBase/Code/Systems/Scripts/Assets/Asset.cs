using Sandbox;

namespace VNBase.Assets;

/// <summary>
/// An on disk asset that can be used in VNBase scripts.
/// </summary>
public interface IAsset
{
	/// <summary>
	/// The path to the asset on disk.
	/// </summary>
	public string Path { get; set; }
}

/// <summary>
/// A base class for all asset game resources.
/// You should still mark asset classes that inherit from this with [GameResource(...)]
/// </summary>
public class AssetResource : GameResource, IAsset
{
	[Hide]
	public string Path { get => ResourcePath; set => ResourcePath = value; }
}
