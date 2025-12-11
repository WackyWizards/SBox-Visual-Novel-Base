using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using VNScript;

namespace VNBase.UI;

public partial class VNHud
{
	[Property, RequireComponent]
	private ScriptPlayer? Player { get; set; }
	
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
			ScriptPlayer.Log.Warning( "No ScriptPlayer assigned to VNHud and VNHud could not find a ScriptPlayer in the scene!" );
		}
	}
	
	public T? GetSubPanel<T>() where T : SubPanel
	{
		return Panel.ChildrenOfType<T>().FirstOrDefault();
	}
	
	private void StyleHack()
	{
		var root = Panel.FindRootPanel();
		root.StyleSheet.Load( "/UI/VNHud.razor.scss" );
	}
	
	/// <summary>
	/// Elements to not allow player passthrough from. <br/>
	/// For example, clicking on a button shouldn't advance the script.
	/// </summary>
	private static readonly Type[] IgnoredAdvancePassthroughElements = [typeof(Button), typeof(DropDown)];
	
	// If the user clicks on the screen, allow advancing the dialogue.
	protected override void OnMouseDown( MousePanelEvent e )
	{
		if ( Player is null )
		{
			return;
		}
		
		// Clicking certain UI elements shouldn't advance.
		if ( IgnoredAdvancePassthroughElements.Contains( e.Target.GetType() ) )
		{
			return;
		}
		
		Player.AdvanceOrSkipDialogueEffect();
	}
	
	protected override int BuildHash()
	{
		return HashCode.Combine( Player, Player?.IsScriptActive, Player?.ActiveScript, Player?.State.GetHashCode() );
	}
}
