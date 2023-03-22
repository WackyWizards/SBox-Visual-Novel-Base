using Sandbox;
using Sandbox.UI;
using System;
using VNBase.UI;

namespace Sandbox;

[Library]
public partial class VNHud : HudEntity<RootPanel>
{
	public VNHud()
	{
		if ( Game.IsServer ) return;

		Rebuild();
	}

	[Event.Hotload]
	public void Rebuild()
	{
		if ( Game.IsServer ) return;

		RootPanel.DeleteChildren();

		try
		{
			var hudElements = TypeLibrary.GetAttributes<HudAttribute>();
			foreach ( var element in hudElements )
			{
				var instance = TypeLibrary.Create<Panel>( element.TargetType );
				if ( instance == null ) continue;
				RootPanel.AddChild( instance );
			}
		}
		catch ( NullReferenceException )
		{
			Log.Error( $"NullReferenceException thrown in: {GetType().Name}" );
		}
	}
}
