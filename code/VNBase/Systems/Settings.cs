using System.Text.Json.Serialization;
using static VNBase.Effects;

namespace VNBase;

public class VNSettings
{
	/// <summary>
	/// The currently active text effect.
	/// </summary>
	[JsonIgnore]
	public ITextEffect TextEffect { get; set; } = new Typewriter();

	/// <summary>
	/// Time used for the active text effect to determine text delays.
	/// </summary>
	public int TextEffectDelay { get; set; } = 55;

	/// <summary>
	/// The action to skip the currently active text effect.
	/// </summary>
	public string SkipAction { get; set; } = "jump";

	/// <summary>
	/// Path to the background images.
	/// </summary>
	public static string BackgroundsPath { get; set; } = "/materials/vnbase/scripts/backgrounds/";
}
