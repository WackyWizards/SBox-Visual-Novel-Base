using System.Linq;
using System.Collections.Generic;
using SandLang;

namespace VNBase;

sealed partial class ScriptPlayer
{
	public List<string> DialogueChoices { get; private set; } = new();

	public void SetChoices( IEnvironment environment, List<Dialogue.Choice> choices )
	{
		DialogueChoices = choices.Where( x => x.IsAvailable( environment ) ).Select( x => x.Text.Format( environment ) ).ToList();
	}

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
