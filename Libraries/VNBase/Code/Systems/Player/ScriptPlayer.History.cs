using VNScript;
using System.Collections.Generic;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	/// <summary>
	/// All previously shown dialogue entries
	/// </summary>
	public List<HistoryEntry> DialogueHistory { get; } = [];
	
	/// <summary>
	/// Adds a dialogue entry to history
	/// </summary>
	private void AddToDialogueHistory( Script.Dialogue dialogue, Script.Label label )
	{
		DialogueHistory.Add( new HistoryEntry( dialogue, label ) );
	}
	
	/// <summary>
	/// Represents dialogue that was actually displayed to the player
	/// </summary>
	public readonly struct HistoryEntry( Script.Dialogue dialogue, Script.Label label )
	{
		public Script.Dialogue Dialogue { get; } = dialogue;
		public Script.Label Label { get; } = label;
	}
}
