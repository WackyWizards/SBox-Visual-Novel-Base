using Sandbox;
using Sandbox.UI;

namespace VNBase.UI;

public partial class MainHud
{
	public static MainHud Current { get; private set; }
	private static Pawn Player => Game.LocalPawn as Pawn;

	public Panel MainPanel { get; private set; }

	public MainHud()
	{
		Current = this;
	}

	protected override void OnAfterTreeRender( bool firstTime )
	{
		if ( !firstTime )
			return;

		base.OnAfterTreeRender( firstTime );
	}
}
