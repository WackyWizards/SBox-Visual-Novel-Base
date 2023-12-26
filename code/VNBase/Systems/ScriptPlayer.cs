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

	[Property] public string ActiveDialogueText { get; set; }

	[Property] public string ActiveBackground { get; set; }

	[Property] public bool TextEffectPlaying { get; set; }

	[Property] public List<CharacterBase> ActiveCharacters { get; set; }

	[Property] public List<string> DialogueHistory { get; set; }

	public VNSettings Settings { get; private set; } = new();

	private Dialogue _dialogue;
	private Dialogue.Label _currentLabel;

	private CancellationTokenSource _cancellationTokenSource;

	protected override void OnStart()
	{
		LoadScript( new ExampleScript() );
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed(Settings.SkipAction) )
			SkipDialogue();
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

	private async void SetCurrentLabel( Dialogue.Label label )
	{
		_currentLabel = label;

		ActiveCharacters.Clear();
		if ( !label.Characters.IsNullOrEmpty() )
		{
			label.Characters.ForEach( ActiveCharacters.Add );
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
				//Sound.FromScreen( sound.Path );
			}
		}

		if ( label.Assets.OfType<BackgroundAsset>().Any() )
		{
			try
			{
				ActiveBackground = label.Assets.OfType<BackgroundAsset>().SingleOrDefault().Path;
			}
			catch ( InvalidOperationException )
			{
				Log.Error( $"There can only be one BackgroundAsset in a Label!" );
			}
		}
		else
		{
			ActiveBackground = null;
		}

		_cancellationTokenSource = new();

		TextEffectPlaying = true;
		try
		{
			await Settings.ActiveTextEffect.Play( label.Text, Settings.TextEffectDelay, ( text ) => ActiveDialogueText = text, _cancellationTokenSource.Token );
		}
		catch ( OperationCanceledException )
		{
			ActiveDialogueText = label.Text;
		}
		TextEffectPlaying = false;

		AddHistory( label.Text );

		// Load our choices.
		ActiveDialogueChoices = label.Choices != null
			? label.Choices
				.Where( p => p.Condition == null ||
					 p.Condition.Execute( GetEnvironment() ) is Value.NumberValue { Number: > 0 } )
				.Select( p => p.ChoiceText )
				.ToList()
			: ContinueChoice;
	}

	public void SkipDialogue()
	{
		if ( TextEffectPlaying )
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

	protected void AddHistory( string dialogue )
	{
		if ( !DialogueHistory.Contains( dialogue ) )
		{
			DialogueHistory.Add( dialogue );
		}
	}

	private static void ScriptLog( object msg, SeverityLevel level = SeverityLevel.Info )
	{
		switch ( level ) 
		{
			case SeverityLevel.Error:
				Log.Error( $"[VNBASE] {msg}" );
				break;
			case SeverityLevel.Warning:
				Log.Warning( $"[VNBASE] {msg}" );
				break;
			default:
				Log.Info($"[VNBASE] {msg}" );
				break;
		}
	}

	private enum SeverityLevel
	{
		Info,
		Warning,
		Error
	}
}
