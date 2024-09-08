using Sandbox;
using System.Text.Json.Serialization;

namespace VNBase;

[GameResource( "character", "char", "Defines a VNBase character." )]
public class Character : GameResource
{
	/// <summary>
	/// The name of the character.
	/// </summary>
	public virtual string Name { get; set; } = string.Empty;

	/// <summary>
	/// The title of the character.
	/// If blank, we assume no title.
	/// </summary>
	public virtual string? Title { get; set; }

	/// <summary>
	/// The color of the character's name.
	/// </summary>
	public virtual Color NameColor { get; set; } = Color.White;

	/// <summary>
	/// The color of the character's title.
	/// </summary>
	public virtual Color TitleColor { get; set; } = Color.White;

	/// <summary>
	/// The name of the active portrait image.
	/// Includes extension.
	/// </summary>
	[JsonIgnore, Hide]
	public string? ActivePortrait { get; set; }

	/// <summary>
	/// Path to the active portrait.
	/// </summary>
	[JsonIgnore, Hide]
	public string ActivePortraitPath => Settings.CharacterPortraitsPath + ActivePortrait;
}
