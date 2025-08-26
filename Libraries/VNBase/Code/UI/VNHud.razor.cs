using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using VNScript;

namespace VNBase.UI;

public partial class VNHud
{
	[Property, RequireComponent] private ScriptPlayer? Player { get; set; }

	private Script.Input? Input => Player?.ActiveLabel?.ActiveInput;
	private bool ShouldShowInput => Player?.State.IsDialogueFinished == true && Input is not null;

	protected override void OnStart()
	{
		// This is an ugly hack to replace default FP styles
		StyleHack();

		try
		{
			Player = Scene.GetAllComponents<ScriptPlayer>().First();
		}
		catch ( InvalidOperationException )
		{
			ScriptPlayer.Log.Warning(
				"No ScriptPlayer assigned to VNHud and VNHud could not find a ScriptPlayer in the scene!" );
		}
	}

	public T? GetSubPanel<T>() where T : SubPanel
	{
		return Panel.ChildrenOfType<T>().FirstOrDefault();
	}

	private void StyleHack()
	{
		var root = Panel.FindRootPanel();
		foreach ( var stylesheet in root.AllStyleSheets.ToList() )
		{
			root.StyleSheet.Remove( stylesheet );
		}

		root.StyleSheet.Load( "/UI/VNHud.razor.scss" );
	}
	
	// If the user clicks on the screen, allow advancing the dialogue.
	protected override void OnMouseDown( MousePanelEvent e )
	{
		if ( Player is null )
		{
			return;
		}

		// Clicking UI buttons shouldn't continue.
		if ( e.Target.GetType() == typeof(Button) )
		{
			return;
		}

		// If the dialogue isn't finished, skip the effect, otherwise just advance if we can.
		if ( !Player.State.IsDialogueFinished )
		{
			Player.SkipDialogueEffect();
		}
		else if ( Player.State.Choices.Count == 0 )
		{
			Player.AdvanceText();
		}
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Player, Player?.IsScriptActive, Player?.ActiveScript, Player?.State.GetHashCode() );
	}
}
