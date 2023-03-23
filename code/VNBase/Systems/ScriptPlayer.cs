using Sandbox;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using SandLang;
using static VNBase.Effects;

namespace VNBase;

/// <summary>
/// Responsible for handling visual novel base scripts.
/// </summary>
public partial class ScriptPlayer : BaseNetworkable
{
	[Net] public ScriptBase ActiveScript { get; set; }
	[Net] public CharacterBase ActiveCharacter { get; set; }

	// at the moment, dialogue attributes have to be networked as separate primitives because I couldn't figure out
	// how to get network serialization of structs working :P
	[Net] public string CurrentDialogueText { get; set; }
	[Net] public bool IsTyping { get; set; }

	private CancellationTokenSource _cancellationToken;

	private Dialogue _dialogue = null;
	private Dialogue.Label _currentLabel = null;

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

		_cancellationToken = new();

		IsTyping = true;
		try
		{
			await Typewriter.Play( label.Text, 70, ( text ) => CurrentDialogueText = text, _cancellationToken.Token );
		}
		catch ( OperationCanceledException )
		{
			CurrentDialogueText = label.Text;
		}
		IsTyping = false;

		CurrentDialogueChoices = label.Choices != null
			? label.Choices
				.Where( p =>p.Condition == null ||
					 p.Condition.Execute( GetEnvironment() ) is Value.NumberValue { Number: > 0 } )
				.Select( p => p.ChoiceText )
				.ToList()
			// if no choices are available, we create "Continue..." which will just direct toward afterlabel
			: ContinueChoice;

		CurrentDialogueChoice = 0;
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

	private static readonly List<string> ContinueChoice = new List<string>( new[] { "Continue..." } );

	private IEnvironment GetEnvironment()
	{
		return new EnvironmentMap( new Dictionary<string, Value>()
		{
			["iter-count"] = new Value.NumberValue( _iterationCount )
		} );
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
