using static VNBase.Effects;

namespace VNBase;

public static class VNSettings
{
	/// <summary>
	/// The currently active text effect
	/// </summary>
	public static ITextEffect ActiveTextEffect { get; set; } = new Typewriter();

	/// <summary>
	/// Path to the background images
	/// </summary>
	public static string BackgroundsPath { get; set; } = "/materials/vnbase/images/backgrounds";
}
