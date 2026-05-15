using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace VNBase.UI;

public partial class DialogueHistory
{
	private bool IsInputPressed => Settings?.HistoryInputs.Any( x => x.Pressed ) ?? false;
	
	private Panel? _historyPanel;
	
	public bool TryScrollHistoryToBottom()
	{
		if ( !_historyPanel.IsValid() )
		{
			return false;
		}
		
		return _historyPanel.TryScrollToBottom();
	}

	public override void Tick()
	{
		if ( IsInputPressed )
		{
			ToggleVisibility();
		}
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Player?.DialogueHistory.Count );
	}
}
