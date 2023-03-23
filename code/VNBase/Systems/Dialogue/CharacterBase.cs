using Sandbox;

namespace VNBase;

public partial class CharacterBase : BaseNetworkable
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
	/// Images path to the character's portraits.
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
	/// The name of the active portrait.
	/// If blank, we assume no portrait.
	/// </summary>
	[Net] public string ActivePortrait { get; set; } = "";
}
