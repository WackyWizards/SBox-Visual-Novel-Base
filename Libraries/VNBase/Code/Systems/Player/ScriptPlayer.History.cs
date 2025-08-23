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
	private void AddToHistory( string text, Script.Label label )
	{
		DialogueHistory.Add( new HistoryEntry( text, label ) );
	}
	
	/// <summary>
	/// Represents a single dialogue entry that was actually displayed to the player
	/// </summary>
	public readonly struct HistoryEntry( string text, Script.Label label )
	{
		public string Text { get; } = text;
		public Script.Label Label { get; } = label;
	}
}
