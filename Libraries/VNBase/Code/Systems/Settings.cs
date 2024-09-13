using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static VNBase.Effects;

namespace VNBase;

/// <summary>
/// Settings for the script reader.
/// </summary>
[Serializable]
public class Settings
{
	/// <summary>
	/// The currently active text effect.
	/// </summary>
	[JsonIgnore]
	[Category( "Text Effect" )]
	public ITextEffect TextEffect { get; set; } = new Typewriter();

	/// <summary>
	/// Time used for the active text effect to determine text delays.
	/// </summary>
	[Category( "Text Effect" )]
	public int TextEffectDelay { get; set; } = 55;

	/// <summary>
	/// The actions to skip the currently active text effect.
	/// </summary>
	[Category( "Skip Action" )]
	public List<SkipInput> SkipActions { get; set; } = new()
	{
		new SkipInput("jump")
	};

	/// <summary>
	/// If we are able to skip the active text effect using a skip action.
	/// </summary>
	[Category( "Skip Action" )]
	public bool SkipActionEnabled { get; set; } = true;

	/// <summary>
	/// Path to the background image assets.
	/// </summary>
	public const string BackgroundsPath = "/materials/scripts/backgrounds/";

	/// <summary>
	/// Path to character portrait images.
	/// </summary>
	public const string CharacterPortraitsPath = "/materials/scripts/";

	/// <summary>
	/// Path to the character resources.
	/// </summary>
	public const string CharacterResourcesPath = "/characters/";
}

public record SkipInput
{
	[InputAction]
	public string Action { get; set; } = string.Empty;

	public SkipInput() { }

	public SkipInput( string action ) => Action = action;
}
