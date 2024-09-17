using System.Collections.Generic;

namespace VNBase;

sealed partial class ScriptPlayer
{
	public List<string> DialogueChoices { get; private set; } = new();

	public void ExecuteChoice( int choiceIndex )
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

		var choice = ActiveLabel.Choices[choiceIndex];
		if ( choice.IsAvailable( ActiveScript.GetEnvironment() ) )
		{
			SetLabel( _activeDialogue.Labels[choice.TargetLabel] );
		}
	}
}
