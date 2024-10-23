using System.Collections.Generic;
using VNBase.Assets;
using SandLang;

namespace VNBase;

/// <summary>
/// Contains a structure for the current active script state.
/// </summary>
public class ScriptState
{
	/// <summary>
	/// The currently active script label text.
	/// </summary>
	public string? DialogueText { get; set; }

	/// <summary>
	/// Path to the currently active background image.
	/// </summary>
	public string? Background { get; set; }

	/// <summary>
	/// The currently active speaking character.
	/// </summary>
	public Character? SpeakingCharacter { get; set; }

	/// <summary>
	/// Characters to display for this label.
	/// </summary>
	public List<Character> Characters { get; set; } = new();

	/// <summary>
	/// The choices for this dialogue.
	/// </summary>
	public List<Dialogue.Choice> Choices { get; set; } = new();

	/// <summary>
	/// Clears the active ScriptState.
	/// </summary>
	public void Clear()
	{
		DialogueText = null;
		SpeakingCharacter = null;
		Background = null;
		Characters.Clear();
		Choices.Clear();
	}
}

