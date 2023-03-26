using static VNBase.Effects;

namespace VNBase;

public class VNSettings
{
	/// <summary>
	/// The currently active text effect
	/// </summary>
	public ITextEffect ActiveTextEffect { get; set; } = new Typewriter();

	public int TextEffectDelay { get; set; } = 70;

	/// <summary>
	/// Path to the background images
	/// </summary>
	public static string BackgroundsPath { get; set; } = "/materials/vnbase/images/backgrounds";
}
