using Sandbox;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using SandLang;

namespace VNBase;

/// <summary>
/// Responsible for handling visual novel base scripts.
/// </summary>
public partial class ScriptPlayer : BaseNetworkable
{
	[Net] public Pawn Owner { get; set; }
	[Net] public ScriptBase ActiveScript { get; set; }
	[Net] public CharacterBase ActiveCharacter { get; set; }
	[Net, Change] public string ActiveDialogueText { get; set; }
	[Net] public string ActiveBackground { get; set; }
	[Net] public bool IsTyping { get; set; }

	public VNSettings Settings { get; set; }

	private CancellationTokenSource _cancellationToken;

	private Dialogue _dialogue = null;
	private Dialogue.Label _currentLabel = null;

	public ScriptPlayer()
	{
		Settings = new VNSettings();
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
		script.Before();

		_dialogue = Dialogue.ParseDialogue(
			SParen.ParseText( script.Dialogue ).ToList()
		);

		SetCurrentLabel( _dialogue.InitialLabel );
	}

	private async void SetCurrentLabel( Dialogue.Label label )
	{
		_currentLabel = label;
		ActiveCharacter = label.Character;

		if ( ActiveCharacter != null )
			ActiveCharacter.ActivePortrait = label.CharacterExpression;

		if ( label.SoundAssets != null )
		{
			foreach ( SoundAsset sound in label.SoundAssets )
			{
				ScriptLog( $"Playing sound: {sound.Path}" );

				// Running To.Single() here doesn't work, why??
				Sound.FromScreen( sound.Path );
			}
		}

		if ( label.Background != null ) 
			ActiveBackground = label.Background.Path;
		else
			ActiveBackground = null;

		_cancellationToken = new();

		IsTyping = true;
		try
		{
			await Settings.ActiveTextEffect.Play( label.Text, Settings.TextEffectDelay, ( text ) => ActiveDialogueText = text, _cancellationToken.Token );
		}
		catch ( OperationCanceledException )
		{
			ActiveDialogueText = label.Text;
		}
		IsTyping = false;

		ActiveDialogueChoices = label.Choices != null
			? label.Choices
				.Where( p =>p.Condition == null ||
					 p.Condition.Execute( GetEnvironment() ) is Value.NumberValue { Number: > 0 } )
				.Select( p => p.ChoiceText )
				.ToList()
			// if no choices are available, we create "Continue..." which will just direct toward afterlabel
			: ContinueChoice;

		ActiveDialogueChoice = 0;
	}

	[ConCmd.Server("dialogue_skip")]
	public static void SkipDialogue()
	{
		var pawn = ConsoleSystem.Caller.Pawn as Pawn;

		var scriptPlayer = pawn?.VNScriptPlayer;
		if ( scriptPlayer == null ) 
		{ 
			ScriptLog( "Unable to skip, no script player found in caller!", SeverityLevel.Error );
			return;
		}

		if ( scriptPlayer.IsTyping )
		{
			scriptPlayer._cancellationToken.Cancel();
			ScriptLog( "Dialogue effect skipped." );
		}
	}

	/// <summary>
	/// Example of a variable read and written using IEnvironment
	/// </summary>
	private int _iterationCount = 0;

	private static readonly List<string> ContinueChoice = new( new[] { "Continue..." } );

	private IEnvironment GetEnvironment()
	{
		return new EnvironmentMap( new Dictionary<string, Value>()
		{
			["self-pawn"] = new Value.WrapperValue<Pawn>( Owner ),
			["iter-count"] = new Value.NumberValue( _iterationCount )
		} );
	}

	private void OnActiveDialogueTextChanged( string oldText, string newText )
	{
		if ( IsTyping ) 
			return;

		if ( Owner.DialogHistory == null )
			return;

		if ( Owner.DialogHistory.Contains( oldText ) )
			return;

		Owner.DialogHistory.Add( oldText );
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
