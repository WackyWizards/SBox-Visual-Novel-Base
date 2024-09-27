using System.Collections.Generic;
using SandLang;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	public List<Dialogue.Choice> DialogueChoices { get; private set; } = new();

	public void ExecuteChoice( Dialogue.Choice choice )
	{
		if ( ActiveScript is null || ActiveLabel is null )
		{
			Log.Warning( "Unable to execute choice: No active script or label." );
			return;
		}

		if ( _activeDialogue is null )
		{
			Log.Error( "Unable to execute choice: No active dialogue." );
			return;
		}

		Dialogue.Label targetLabel = _activeDialogue.Labels[choice.TargetLabel];
		if ( choice.IsAvailable( ActiveScript.GetEnvironment() ) )
		{
			SetLabel( targetLabel );
		}
		else
		{
			Log.Error( $"Tried executing choice which isn't available: '{targetLabel.Name}'. This shouldn't be possible!" );
		}
	}
}
