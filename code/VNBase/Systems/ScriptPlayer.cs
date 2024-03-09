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
	[Property] public Script ActiveScript { get; private set; }

	[Property] public string DialogueText { get; set; }

	[Property] public string Background { get; set; }

	[Property] public bool DialogueFinished { get; set; }

	[Property] public Character SpeakingCharacter { get; set; }

	[Property] public List<Character> Characters { get; set; }

	[Property] public List<Dialogue.Label> DialogueHistory { get; set; }

	[Property] public VNSettings Settings { get; private set; } = new();

	private Dialogue _dialogue;
	private Dialogue.Label _currentLabel;

	private CancellationTokenSource _cts;

	protected override void OnStart()
	{
		LoadScript( "examples/scripts/ExampleLongScript.vnscript" );

		if ( Scene.GetAllComponents<VNHud>().IsNullOrEmpty() )
		{
			ScriptLog( "No VNHud Component found, ScriptPlayer will not be immediately visible!", SeverityLevel.Warning );
		}
	}

	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( Settings.SkipAction ) )
		{
			if ( ActiveScript is null )
				return;

			if ( !DialogueFinished )
			{
				SkipDialogue();
			}
			else if ( DialogueChoices.IsNullOrEmpty() )
				UnloadScript();
		}
	}

	public void LoadScript( string path )
	{
		var dialogue = FileSystem.Mounted.ReadAllText( path );

		if ( dialogue is null )
		{
			ScriptLog( $"Unable to load script! Script file couldn't be found by path: {path}", SeverityLevel.Error );
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
			ScriptLog( "Unable to load script! The script file is empty.", SeverityLevel.Error );
		}
	}

	public void LoadScript( Script script )
	{
		if ( script is null )
		{
			ScriptLog( "Unable to load script! Script is null!", SeverityLevel.Error );
			return;
		}

		ScriptLog( $"Loading script: {script.GetType().Name}" );

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
		if ( ActiveScript is null )
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

		ScriptLog( $"Unloaded active script." );
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
			ScriptLog( $"There can only be one BackgroundAsset in a Label!", SeverityLevel.Error );
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

	public void SkipDialogue()
	{
		if ( !DialogueFinished )
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
