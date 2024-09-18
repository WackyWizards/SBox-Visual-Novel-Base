using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static VNBase.Effects;

namespace VNBase;

/// <summary>
/// Settings for the script player.
/// </summary>
[Title( "VN Settings" )]
[Category( "VNBase" )]
public class Settings : Component
{
	[Property, ToggleGroup( "TextEffectEnabled" )]
	public bool TextEffectEnabled { get; set; } = true;

	/// <summary>
	/// The currently active text effect.
	/// </summary>
	[JsonIgnore]
	[Property, ToggleGroup( "TextEffectEnabled" )]
	public ITextEffect TextEffect { get; set; } = new Typewriter();

	/// <summary>
	/// Time used for the active text effect to determine text delays.
	/// </summary>
	[Property, ToggleGroup( "TextEffectEnabled" )]
	public int TextEffectDelay { get; set; } = 55;

	/// <summary>
	/// The actions to skip the currently active text effect.
	/// By default, this is set to "jump".
	/// </summary>
	[InlineEditor]
	[Property, ToggleGroup( "SkipActionEnabled" )]
	public List<SkipInput> SkipActions { get; set; } = new()
	{
		"jump"
	};

	/// <summary>
	/// If we are able to skip the active text effect using a skip action.
	/// </summary>
	[Property, ToggleGroup( "SkipActionEnabled" )]
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

public class SkipInput : IEquatable<InputAction>
{
	[InputAction]
	public string Action { get; set; } = string.Empty;

	public bool Equals( InputAction? other )
	{
		return Action == other?.Name;
	}

	public static implicit operator string( SkipInput skipInput ) => skipInput.Action;
	public static implicit operator SkipInput( string action ) => new() { Action = action };
}
