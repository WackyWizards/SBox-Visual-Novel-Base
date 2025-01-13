using Sandbox;
using System.Text.Json.Serialization;

namespace VNBase.Assets;

/// <summary>
/// Defines a VNBase character.
/// </summary>
[GameResource( "Character", "char", "Defines a VNBase character.", Category = "VNBase", Icon = "person" )]
public sealed class Character : AssetResource
{
	/// <summary>
	/// The name of the character.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The title of the character.
	/// If blank, we assume no title.
	/// </summary>
	public string? Title { get; set; }

	/// <summary>
	/// The color of the character's name.
	/// </summary>
	public Color NameColor { get; set; } = Color.White;

	/// <summary>
	/// The color of the character's title.
	/// </summary>
	public Color TitleColor { get; set; } = Color.White;

	/// <summary>
	/// The name of the active portrait image.
	/// Includes extension.
	/// </summary>
	[JsonIgnore, Hide]
	public string? ActivePortrait { get; set; }

	/// <summary>
	/// Path to the active portrait image.
	/// </summary>
	[JsonIgnore, Hide]
	public string ActivePortraitPath => $"{Settings.CharacterPortraitsPath}/{Name}/{ActivePortrait}";
}
