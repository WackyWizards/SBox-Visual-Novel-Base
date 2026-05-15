using VNScript;
using System.Collections.Generic;
using Sandbox;

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
		
		if ( !Hud.IsValid() )
		{
			return;
		}
		
		var dialogueHistoryPanel = Hud.DialogueHistory;
		if ( !dialogueHistoryPanel.IsValid() )
		{
			return;
		}
		
		dialogueHistoryPanel.TryScrollHistoryToBottom();
	}
	
	/// <summary>
	/// Represents dialogue actually displayed to the player
	/// </summary>
	public class HistoryEntry( Script.Dialogue dialogue, Script.Label label )
	{
		public Script.Dialogue Dialogue { get; } = dialogue;
		public Script.Label Label { get; } = label;
	}
}
