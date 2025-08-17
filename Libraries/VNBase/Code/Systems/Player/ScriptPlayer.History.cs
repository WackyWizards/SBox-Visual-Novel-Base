using SandLang;
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
	private void AddToHistory( string text, Dialogue.Label label )
	{
		DialogueHistory.Add( new HistoryEntry( text, label ) );
	}
	
	/// <summary>
	/// Represents a single dialogue entry that was actually displayed to the player
	/// </summary>
	public readonly struct HistoryEntry( string text, Dialogue.Label label )
	{
		public string Text { get; } = text;
		public Dialogue.Label Label { get; } = label;
	}
}
