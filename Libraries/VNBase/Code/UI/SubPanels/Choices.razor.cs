using System;
using System.Linq;
using VNScript;

namespace VNBase.UI;

public partial class Choices
{
#pragma warning disable CA1822
	private bool HasChoices => Player?.State.Choices.Any() == true;
#pragma warning restore CA1822
	
	private void ExecuteChoice( Script.Choice choice )
	{
		Player?.ExecuteChoice( choice );
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Player?.State.DialogueText, Player?.State.IsDialogueFinished, Player?.State.Choices.Count, HasChoices );
	}
}
