using Sandbox;

namespace VNScript;

public partial class Script
{
	/// <summary>
	/// Represents a choice by the player, possible required conditions for it to be a viable choice, and the new label to direct towards.
	/// </summary>
	public class Choice
	{
		public FormattableText Text { get; set; } = string.Empty;

		public string TargetLabel { get; set; } = string.Empty;

		[Hide]
		public SParen? Condition { get; set; }

		/// <summary>
		/// Returns whether this condition is available to the player.
		/// </summary>
		public bool IsAvailable( IEnvironment environment )
		{
			if ( Condition is null )
			{
				return true;
			}

			var value = Condition.Execute( environment );
			if ( value is Value.BooleanValue boolValue )
			{
				return boolValue.Boolean;
			}

			return false;
		}
	}
}
