using Sandbox.UI;

namespace VNBase.UI;

public class SubPanel : Panel
{
	public ScriptPlayer Player { get; set; }

	public void TogglePanel()
	{
		SetClass( "hidden", IsVisible );
	}
}
