using Sandbox;
using System.Collections.Generic;

namespace VNBase;

sealed partial class ScriptPlayer
{
	[Property] public List<string> DialogueChoices { get; private set; } = new();
	public static readonly List<string> ContinueChoice = new( new[] { "Continue" } );

	public void ExecuteChoice( int choiceIndex )
	{
		var choice = _currentLabel?.Choices[choiceIndex];
		if ( choice.IsAvailable( ActiveScript.GetEnvironment() ) )
		{
			SetCurrentLabel( _dialogue.Labels[choice.TargetLabel] );
		}

		ExecuteAfterLabel();
	}
}
