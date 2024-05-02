using Sandbox.UI;

namespace VNBase.UI;

public class SubPanel : Panel
{
	public ScriptPlayer? Player { get; set; }

	public void ToggleVisibility()
	{
		SetClass( "hidden", IsVisible );
	}

	public void Hide()
	{
		AddClass( "hidden" );
	}

	public void Show()
	{
		RemoveClass( "hidden" );
	}
}
