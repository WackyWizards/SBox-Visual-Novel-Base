using Sandbox;

namespace VNBase;

public class Character
{
	/// <summary>
	/// The name of the character.
	/// </summary>
	public virtual string Name { get; set; }

	/// <summary>
	/// The title of the character.
	/// If blank, we assume no title.
	/// </summary>
	public virtual string Title { get; set; }

	/// <summary>
	/// Path to the folder containing character portraits.
	/// </summary>
	public virtual string Images { get; set; } = "/materials/vnbase/scripts/";

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
	public string ActivePortrait { get; set; }

	/// <summary>
	/// Path to the active portrait.
	/// </summary>
	public string ActivePortraitPath => Images + ActivePortrait;
}
