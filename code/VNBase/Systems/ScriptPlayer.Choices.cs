using Sandbox;
using System.Collections.Generic;

namespace VNBase;

sealed partial class ScriptPlayer
{
	public List<string> DialogueChoices { get; private set; } = new();
	public static readonly List<string> ContinueChoice = new( new[] { "Continue" } );

	public void ExecuteChoice( int choiceIndex )
	{
		if ( ActiveScript is null || _dialogue is null || _currentLabel is null )
		{
			return;
		}

		var choice = _currentLabel.Choices[choiceIndex];
		if ( choice.IsAvailable( ActiveScript.GetEnvironment() ) )
		{
			SetCurrentLabel( _dialogue.Labels[choice.TargetLabel] );
		}

		ExecuteAfterLabel();
	}
}
