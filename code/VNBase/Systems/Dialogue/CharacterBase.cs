using Sandbox;

namespace VNBase;

public class CharacterBase : BaseNetworkable
{
	/// <summary>
	/// The name of the character.
	/// </summary>
	public virtual string Name => "No Name";

	/// <summary>
	/// The title of the character.
	/// </summary>
	public virtual string Title => "No Title";

	/// <summary>
	/// Images path to the character's portraits.
	/// If blank, we search for the character's name in materials/vnbase/images.
	/// </summary>
	public virtual string Images => "/materials/vnbase/images/";

	/// <summary>
	/// The color of the character's name.
	/// </summary>
	public virtual Color NameColor => Color.White;

	/// <summary>
	/// The color of the character's title.
	/// </summary>
	public virtual Color TitleColor => Color.White;
}
