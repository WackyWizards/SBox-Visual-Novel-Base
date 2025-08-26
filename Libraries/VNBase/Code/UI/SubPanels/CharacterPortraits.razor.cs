using System;
using System.Linq;

namespace VNBase.UI;

public partial class CharacterPortraits
{
#pragma warning disable CA1822
	private bool HasCharacters => Player?.State.Characters.Any() == true;
#pragma warning restore CA1822
	
	protected override int BuildHash()
	{
		return HashCode.Combine( Player?.State.DialogueText, Player?.State.Characters.Count, HasCharacters );
	}
}
