using Sandbox;
using System.Linq;
using System.Collections.Generic;
using SandLang;

namespace VNBase;

partial class ScriptPlayer
{
	[Net] public IList<string> CurrentDialogueChoices { get; set; }
	public int CurrentDialogueChoice { get; set; }

	// We use a command to trigger dialogue execution
	[ConCmd.Server( "dialogue_choice" )]
	public static void DialogueChoice( int choice )
	{
		var pawn = ConsoleSystem.Caller.Pawn as Pawn;

		pawn!.VNScriptPlayer.ExecuteChoice( choice );
	}

	private void ExecuteChoice( int choiceIndex )
	{
		if ( Game.IsServer )
		{
			var choice = _currentLabel.Choices?[choiceIndex];
			var dialogueEnvironment = GetEnvironment();
			// when no choices are available, just "Continue" will be an option and it will execute "AfterLabel".
			if ( choice == null && _currentLabel.Choices == null )
			{
				var afterLabel = _currentLabel.AfterLabel;
				foreach ( var codeBlock in afterLabel.CodeBlocks )
				{
					codeBlock.Execute( dialogueEnvironment );
				}

				if ( afterLabel.TargetLabel == null )
				{
					_currentLabel = null;
					CurrentDialogueChoices = null;
					CurrentDialogueText = null;
					_dialogue = null;

					ActiveScript?.After();
					if ( ActiveScript.NextScript != null ) 
						LoadScript( ActiveScript.NextScript );
					else
						ActiveScript = null;
				}
				else
				{
					SetCurrentLabel( _dialogue.DialogueLabels[afterLabel.TargetLabel] );
				}
			}
			else if ( choice != null &&
					  (
						  choice.Condition == null ||
						  (choice.Condition.Execute( dialogueEnvironment ) as Value.NumberValue)!
						  .Number > 0
					  )
					)
			{
				SetCurrentLabel( _dialogue.DialogueLabels[choice.TargetLabel] );
			}
		}
	}
}
