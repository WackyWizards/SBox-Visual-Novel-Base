using Sandbox;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using SandLang;
using VNBase.Util;

namespace VNBase;

/// <summary>
/// Responsible for handling visual novel base scripts.
/// </summary>
[Title("VN Script Player")]
public partial class ScriptPlayer : Component
{
	[Property] public ScriptBase ActiveScript { get; private set; }

	[Property] public CharacterBase SpeakingCharacter { get; set; }

	[Property] public string DialogueText { get; set; }

	[Property] public string Background { get; set; }

	[Property] public bool DialogueFinished { get; set; }

	[Property] public List<CharacterBase> Characters { get; set; }

	[Property] public List<Dialogue.Label> DialogueHistory { get; set; }

	public VNSettings Settings { get; private set; } = new();

	private Dialogue _dialogue;
	private Dialogue.Label _currentLabel;

	private CancellationTokenSource _cancellationTokenSource;

	protected override void OnStart()
	{
		LoadScript( new ExampleScript() );
	}

	protected override void OnFixedUpdate()
	{
		if ( Input.Pressed( Settings.SkipAction ) )
		{
			if ( !DialogueFinished )
			{
				SkipDialogue();
			}
			else if ( ActiveDialogueChoices.IsNullOrEmpty() )
				UnloadScript();
		}
	}

	public void LoadScript( ScriptBase script ) 
	{
		if ( script == null ) 
		{
			ScriptLog( "Unable to load script! No script found.", SeverityLevel.Error );
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
		_currentLabel = null;

		ActiveDialogueChoices = null;
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
		if ( !label.Characters.IsNullOrEmpty() )
		{
			label.Characters.ForEach( Characters.Add );
		}

		if ( label.SpeakingCharacter != null )
		{
			SpeakingCharacter = label.SpeakingCharacter;
		}
		else
		{
			SpeakingCharacter = null;
		}

		if ( label.Assets.OfType<SoundAsset>().Any() )
		{
			foreach ( SoundAsset sound in label.Assets.OfType<SoundAsset>() )
			{
				Sound.Play( sound.Path );
			}
		}

		if ( label.Assets.OfType<BackgroundAsset>().Any() )
		{
			try
			{
				Background = label.Assets.OfType<BackgroundAsset>().SingleOrDefault().Path;
			}
			catch ( InvalidOperationException )
			{
				Log.Error( $"There can only be one BackgroundAsset in a Label!" );
			}
		}
		else
		{
			Background = null;
		}

		_cancellationTokenSource = new();

		try
		{
			await Settings.ActiveTextEffect.Play( label.Text, Settings.TextEffectDelay, ( text ) => DialogueText = text, _cancellationTokenSource.Token );
		}
		catch ( OperationCanceledException )
		{
			DialogueText = label.Text;
		}

		AddHistory( label );
		DialogueFinished = true;

		ActiveDialogueChoices = null;
		if ( label.Choices != null )
		{
			// Load our choices, or create a continue choice,
			// if we don't have any valid ones.
			ActiveDialogueChoices = label.Choices != null
				? label.Choices
					.Where( p => p.Condition == null ||
						 p.Condition.Execute( GetEnvironment() ) is Value.NumberValue { Number: > 0 } )
					.Select( p => p.ChoiceText )
					.ToList()
				: ContinueChoice;
		}
	}

	public void SkipDialogue()
	{
		if ( !DialogueFinished )
		{
			_cancellationTokenSource.Cancel();
		}
	}

	private IEnvironment GetEnvironment()
	{
		// We use an EnvironmentMap to map unique variables for use in S-Expression code.
		return new EnvironmentMap( new Dictionary<string, Value>()
		{

		});
	}

	protected void AddHistory( Dialogue.Label dialogue )
	{
		if ( !DialogueHistory.Contains( dialogue ) )
		{
			DialogueHistory.Add( dialogue );
		}
	}
}
