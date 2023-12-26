using Sandbox;
using System.Collections.Generic;
using SandLang;

namespace VNBase;

partial class ScriptPlayer
{
	[Property] public List<string> ActiveDialogueChoices { get; private set; }

	public static readonly List<string> ContinueChoice = new( new[] { "Continue" } );

	public void ExecuteChoice( int choiceIndex )
	{
		if ( _currentLabel == null ) return;

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
