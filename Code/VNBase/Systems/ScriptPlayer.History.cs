using SandLang;
using System.Collections.Generic;

namespace VNBase;

sealed partial class ScriptPlayer
{
	/// <summary>
	/// All previously shown labels.
	/// </summary>
	public List<Dialogue.Label> DialogueHistory { get; set; } = new();

	private void AddHistory( Dialogue.Label dialogue )
	{
		if ( !DialogueHistory.Contains( dialogue ) )
		{
			DialogueHistory.Add( dialogue );
		}
	}
}
