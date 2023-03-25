using Sandbox;

namespace VNBase;

/// <summary>
/// An asset that can be used in VNBase scripts.
/// </summary>
public class AssetBase
{
	public interface IAsset
	{
		public string Path { get; set; }
	}
}
