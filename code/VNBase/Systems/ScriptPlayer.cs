using Sandbox;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using VNBase.Assets;
using VNBase.Util;
using VNBase.UI;
using SandLang;

namespace VNBase;

/// <summary>
/// Responsible for handling visual novel base scripts.
/// </summary>
[Title( "VN Script Player" )]
[Category("VNBase")]
public sealed partial class ScriptPlayer : Component
{
	/// <summary>
	/// If not empty, will load the script at this path on initial component start.
	/// </summary>
	[Property] public string InitialScript { get; set; }

	/// <summary>
	/// The currently active script.
	/// </summary>
	[Property] public Script ActiveScript { get; private set; }

	/// <summary>
	/// The currently active script label text.
	/// </summary>
	[Property] public string DialogueText { get; set; }

	/// <summary>
	/// Path to the currently active background.
	/// </summary>
	[Property] public string Background { get; set; }

	/// <summary>
	/// If the dialogue has finished writing text.
	/// </summary>
	[Property] public bool DialogueFinished { get; set; }

	/// <summary>
	/// The currently active speaking character.
	/// </summary>
	[Property] public Character SpeakingCharacter { get; set; }

	/// <summary>
	/// Characters to display for this label.
	/// </summary>
	[Property] public List<Character> Characters { get; set; }

	/// <summary>
	/// History of all previously shown labels.
	/// </summary>
	[Property] public List<Dialogue.Label> DialogueHistory { get; set; }

	[Property] public VNSettings Settings { get; private set; } = new();

	private Dialogue _dialogue;
	private Dialogue.Label _currentLabel;

	private CancellationTokenSource _cts;

	protected override void OnStart()
	{
		if ( InitialScript != null )
		{
			LoadScript( InitialScript );
		}

		if ( Scene.GetAllComponents<UI.VNHud>().IsNullOrEmpty() )
		{
			Log.Warning( "No VNHud Component found, ScriptPlayer will not be immediately visible!");
		}
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( Settings.SkipAction ) )
		{
			if ( ActiveScript == null )
				return;

			if ( !DialogueFinished )
			{
				SkipDialogue();
			}
			else if ( DialogueChoices.IsNullOrEmpty() )
				UnloadScript();
		}
	}

	/// <summary>
	/// Read and Load the script at the provided path.
	/// </summary>
	/// <param name="path">Path to the script to load.</param>
	public void LoadScript( string path )
	{
		var dialogue = FileSystem.Mounted.ReadAllText( path );

		if ( dialogue == null )
		{
			Log.Error( $"Unable to load script! Script file couldn't be found by path: {path}" );
			return;
		}

		if ( !string.IsNullOrEmpty( dialogue ) )
		{
			Script script = new()
			{
				Dialogue = dialogue
			};
			LoadScript( script );
		}
		else
		{
			Log.Error("Unable to load script! The script file is empty.");
		}
	}

	/// <summary>
	/// Load the provided Script object.
	/// </summary>
	/// <param name="script">Script to load.</param>
	public void LoadScript( Script script )
	{
		if ( script == null )
		{
			Log.Error("Unable to load script! Script is null!");
			return;
		}

		Log.Info( $"Loading script: {script.GetType().Name}" );

		ActiveScript = script;
		script.OnLoad();

		_dialogue = Dialogue.ParseDialogue(
			SParen.ParseText( script.Dialogue ).ToList()
		);

		SetCurrentLabel( _dialogue.InitialLabel );
	}

	/// <summary>
	/// Unloads the currently active script.
	/// </summary>
	public void UnloadScript()
	{
		if ( ActiveScript == null )
		{
			return;
		}

		_dialogue = null;
		DialogueChoices?.Clear();
		DialogueText = null;
		SpeakingCharacter = null;
		Background = null;

		ActiveScript.After();
		if ( ActiveScript.NextScript != null )
		{
			LoadScript( ActiveScript.NextScript );
		}
		else
		{
			ActiveScript = null;
		}

		Log.Info( $"Unloaded active script." );
	}

	private async void SetCurrentLabel( Dialogue.Label label )
	{
		_currentLabel = label;
		DialogueFinished = false;

		Characters.Clear();
		label.Characters.ForEach( Characters.Add );
		SpeakingCharacter = label.SpeakingCharacter;

		foreach ( SoundAsset sound in label.Assets.OfType<SoundAsset>() )
		{
			Sound.Play( sound.Path );
		}

		try
		{
			Background = label.Assets.OfType<BackgroundAsset>().SingleOrDefault()?.Path;
		}
		catch ( InvalidOperationException )
		{
			Log.Error($"There can only be one BackgroundAsset in a Label!");
			Background = null;
		}

		_cts = new();

		try
		{
			await Settings.TextEffect.Play( label.Text, Settings.TextEffectDelay, ( text ) => DialogueText = text, _cts.Token );
		}
		catch ( OperationCanceledException )
		{
			DialogueText = label.Text;
		}

		AddHistory( label );
		DialogueFinished = true;

		DialogueChoices = label.Choices.Where( x => x.IsAvailable( ActiveScript.GetEnvironment() ) ).Select( p => p.ChoiceText ).ToList();
	}

	private void ExecuteAfterLabel()
	{
		var afterLabel = _currentLabel.AfterLabel;

		if ( afterLabel != null )
		{
			foreach ( var codeBlock in afterLabel.CodeBlocks )
			{
				codeBlock.Execute( ActiveScript.GetEnvironment() );
			}

			if ( afterLabel.TargetLabel != null )
			{
				SetCurrentLabel( _dialogue.Labels[afterLabel.TargetLabel] );
			}
		}
	}

	/// <summary>
	/// Skip the currently active text effect.
	/// </summary>
	public void SkipDialogue()
	{
		if ( !DialogueFinished && _cts != null )
		{
			_cts.Cancel();
		}
	}

	private void AddHistory( Dialogue.Label dialogue )
	{
		if ( !DialogueHistory.Contains( dialogue ) )
		{
			DialogueHistory.Add( dialogue );
		}
	}
}
