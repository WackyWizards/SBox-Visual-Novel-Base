using static VNBase.Effects;

namespace VNBase;

public class VNSettings
{
	/// <summary>
	/// The currently active text effect.
	/// </summary>
	public ITextEffect ActiveTextEffect { get; set; } = new Typewriter();

	/// <summary>
	/// Time used for the active text effect to determine text delays.
	/// </summary>
	public int TextEffectDelay { get; set; } = 55;

	/// <summary>
	/// The action to skip the currently active text effect. You could define your own action in your project
	/// settings and use that here, but we'll bind it to jump for simplicity's sake.
	/// </summary>
	public string SkipAction { get; set; } = "jump";

	/// <summary>
	/// Path to the background images.
	/// </summary>
	public static string BackgroundsPath { get; set; } = "/materials/vnbase/images/backgrounds";
}
