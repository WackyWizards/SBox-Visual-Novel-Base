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
	public const string BackgroundsPath = "/materials/vnbase/scripts/backgrounds/";

	/// <summary>
	/// Path to character portrait images.
	/// </summary>
	public const string CharacterPortraitsPath = "/materials/vnbase/scripts/";

	/// <summary>
	/// Path to the character resources.
	/// </summary>
	public const string CharacterResourcesPath = "/characters/";
}
