using VNBase;

namespace VNScript;

public partial class Script
{
	/// <summary>
	/// Represents an input from the player, and the variable to store the input in.
	/// </summary>
	public class Input
	{
		public string VariableName { get; init; } = string.Empty;

		private IEnvironment? _environment;

		/// <summary>
		/// Sets the value of the input variable in the environment.
		/// </summary>
		/// <param name="environment">The environment to set the value in.</param>
		/// <param name="value">The value to set the variable to.</param>
		public void SetValue( IEnvironment environment, Value value )
		{
			environment.SetVariable( VariableName, value );
			_environment = environment;

			if ( ScriptPlayer.LoggingEnabled )
			{
				Log.Info( $"Set value of variable \"{VariableName}\" to \"{_environment.GetVariable( VariableName )}\" through user input." );
			}
		}
	}
}
