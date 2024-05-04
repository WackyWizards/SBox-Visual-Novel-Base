using Sandbox;
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
	[InputAction]
	public string SkipAction { get; set; } = "jump";

	/// <summary>
	/// If we are able to skip the active text effect using the skip action.
	/// </summary>
	public bool SkipActionEnabled { get; set; } = true;

	/// <summary>
	/// Path to the background image assets.
	/// </summary>
	public static string BackgroundsPath { get; set; } = "/materials/vnbase/scripts/backgrounds/";

	/// <summary>
	/// Path to the sound assets.
	/// </summary>
	public static string SoundsPath { get; set; } = "/sounds/vnbase/scripts/";
}
