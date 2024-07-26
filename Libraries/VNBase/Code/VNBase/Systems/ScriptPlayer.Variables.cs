using SandLang;

namespace VNBase;

sealed partial class ScriptPlayer
{
	internal void SetEnvironment( Dialogue dialogue )
	{
		if ( ActiveScript is null )
		{
			return;
		}

		var environment = ActiveScript.GetEnvironment();

		foreach ( var variable in dialogue.Variables )
		{
			var variableName = ((Value.VariableReferenceValue)variable.Key).Name;

			if ( variable.Value is Value.VariableReferenceValue reference )
			{
				environment.SetVariable( variableName, reference.Evaluate( environment ) );
			}
			else
			{
				environment.SetVariable( variableName, variable.Value );
			}
		}

		//LogVariables( environment );

		_dialogue = dialogue;
	}

	private void LogVariables( IEnvironment environment )
	{
		foreach ( var val in environment.GetVariables() )
		{
			if ( val.Value is Value.ListValue list )
			{
				foreach ( var v in list.ValueList )
				{
					Log.Info( v );
				}
			}

			Log.Info( $"Variable: {val.Key} = {val.Value}" );
		}
	}
}
