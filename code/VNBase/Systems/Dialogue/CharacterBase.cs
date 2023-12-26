using Sandbox;

namespace VNBase;

public class CharacterBase
{
	/// <summary>
	/// The name of the character.
	/// </summary>
	public virtual string Name => "No Name";

	/// <summary>
	/// The title of the character.
	/// If blank, we assume no title.
	/// </summary>
	public virtual string Title => "";

	/// <summary>
	/// Path to the folder containing character portraits.
	/// </summary>
	public virtual string Images => "/materials/vnbase/images";

	/// <summary>
	/// The color of the character's name.
	/// </summary>
	public virtual Color NameColor => Color.White;

	/// <summary>
	/// The color of the character's title.
	/// </summary>
	public virtual Color TitleColor => Color.White;

	/// <summary>
	/// The name of the active portrait image.
	/// Includes extension.
	/// </summary>
	public string ActivePortrait { get; set; }
}
