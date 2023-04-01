using Sandbox;
using System.Collections.Generic;
using SandLang;

namespace VNBase;

partial class ScriptPlayer
{
	[Net] public IList<string> ActiveDialogueChoices { get; set; }
	public int ActiveDialogueChoice { get; set; }

	// We use a command to trigger dialogue execution
	[ConCmd.Server( "dialogue_choice" )]
	public static void DialogueChoice( int choice )
	{
		var pawn = ConsoleSystem.Caller.Pawn as Pawn;

		pawn!.VNScriptPlayer.ExecuteChoice( choice );
	}

	private void ExecuteChoice( int choiceIndex )
	{
		if ( !Game.IsServer ) return;

		var choice = _currentLabel.Choices?[choiceIndex];
		var dialogueEnvironment = GetEnvironment();

		if ( choice == null && _currentLabel.Choices == null )
		{
			ExecuteAfterLabel();
		}
		else if ( choice?.Condition == null || choice.Condition.Execute( dialogueEnvironment ) is Value.NumberValue { Number: > 0 } )
		{
			SetCurrentLabel( _dialogue.DialogueLabels[choice.TargetLabel] );
		}
	}

	private void ExecuteAfterLabel()
	{
		var afterLabel = _currentLabel.AfterLabel;
		foreach ( var codeBlock in afterLabel.CodeBlocks )
		{
			codeBlock.Execute( GetEnvironment() );
		}

		if ( afterLabel.TargetLabel == null )
		{
			_currentLabel = null;
			ActiveDialogueChoices = null;
			ActiveDialogueText = null;
			_dialogue = null;

			ActiveScript.After();
			if ( ActiveScript.NextScript != null )
			{
				LoadScript( ActiveScript.NextScript );
			}
			else
			{
				ActiveScript = null;
			}
		}
		else
		{
			SetCurrentLabel( _dialogue.DialogueLabels[afterLabel.TargetLabel] );
		}
	}
}
