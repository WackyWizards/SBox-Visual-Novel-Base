using Sandbox;

namespace VNBase.Assets;

/// <summary>
/// An asset that can be used in VNBase scripts.
/// </summary>
public class Asset
{
	public interface IAsset
	{
		public string Path { get; set; }
	}
}
