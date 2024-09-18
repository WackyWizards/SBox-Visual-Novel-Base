using SandLang;

namespace VNBase;

sealed partial class ScriptPlayer
{
	internal void SetEnvironment( Dialogue dialogue )
	{
		if ( ActiveScript is null )
		{
			Log.Error( "No active script to set environment for!" );
			return;
		}

		IEnvironment environment = ActiveScript.GetEnvironment();

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

		foreach ( var label in dialogue.Labels.Values )
		{
			label.Environment = environment;
		}

		_environment = environment;
	}

	/// <summary>
	/// Logs all of the variables in the environment.
	/// </summary>
	/// <param name="environment">The environment to log the variables of.</param>
	internal static void LogVariables( IEnvironment environment )
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
