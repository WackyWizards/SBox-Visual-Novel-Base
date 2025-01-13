using SandLang;
using System.Collections.Generic;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	/// <summary>
	/// All previously shown labels.
	/// </summary>
	public List<Dialogue.Label> DialogueHistory { get; } = [];

	private void AddHistory( Dialogue.Label label )
	{
		if ( DialogueHistory.Contains( label ) )
		{
			return;
		}

		label.Text = label.Text.Format( _environment );
		DialogueHistory.Add( label );
	}
}
