using Sandbox;
using Sandbox.UI;

namespace VNBase.UI;

public partial class MainHud
{
	public static MainHud Current { get; private set; }

	public Panel MainPanel { get; private set; }

	public MainHud()
	{
		Current = this;
	}
}
