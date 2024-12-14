using System;
using System.Linq;
using System.Threading;
using VNBase.UI;
using VNBase.Assets;
using SandLang;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	private async void SetLabel( Dialogue.Label label )
	{
		ActiveLabel = label;
		DialogueFinished = false;

		if ( LoggingEnabled )
		{
			Log.Info( $"Loading Label {label.Name}" );
		}

		State.Characters.Clear();
		label.Characters.ForEach( State.Characters.Add );
		State.SpeakingCharacter = label.SpeakingCharacter;

		foreach ( var sound in label.Assets.OfType<Sound>() )
		{
			sound.Play();

			if ( LoggingEnabled )
			{
				Log.Info( $"Played SoundAsset {sound} from label {label.Name}" );
			}
		}

		try
		{
			State.Background = label.Assets.OfType<Background>().SingleOrDefault()?.Path;
		}
		catch ( InvalidOperationException )
		{
			Log.Error( $"There can only be one {nameof( Background )} in label {label.Name}!" );
			State.Background = null;
		}

		_cts = new CancellationTokenSource();

		var environment = _environment;
		var formattedText = label.Text.Format( environment );
		if ( Settings?.TextEffectEnabled ?? false && Settings.TextEffect is not null )
		{
			try
			{
				await Settings.TextEffect.Play( formattedText, Settings.TextEffectDelay, ( text ) => State.DialogueText = text, _cts.Token );
			}
			catch ( OperationCanceledException )
			{
				State.DialogueText = formattedText;
			}
		}
		else
		{
			State.DialogueText = formattedText;
		}

		AddHistory( label );

		// If we are in Automatic Mode, wait a bit
		// before ending the dialogue.
		if ( AutomaticMode && label.Choices.Count == 0 )
		{
			await Task.DelaySeconds( Settings.AutoDelay );
		}

		EndDialogue();
	}

	private void ExecuteAfterLabel()
	{
		if ( ActiveScript is null || ActiveLabel is null )
		{
			Log.Error( $"Unable to execute the AfterLabel, there is either no active script or label!" );
			return;
		}

		var afterLabel = ActiveLabel.AfterLabel;

		if ( afterLabel is null )
		{
			return;
		}

		foreach ( var codeBlock in afterLabel.CodeBlocks )
		{
			codeBlock.Execute( ActiveScript.GetEnvironment() );
		}

		var hasInput = ActiveLabel.ActiveInput is not null;
		if ( hasInput && Hud is not null )
		{
			var input = Hud.GetSubPanel<TextInput>();

			if ( string.IsNullOrWhiteSpace( input.Entry.Text ) )
			{
				return;
			}
		}

		if ( afterLabel.IsLastLabel )
		{
			UnloadScript();
			return;
		}

		if ( afterLabel.ScriptPath is not null )
		{
			LoadScript( afterLabel.ScriptPath );
			return;
		}

		if ( afterLabel.TargetLabel is null )
		{
			return;
		}

		if ( _activeDialogue is null )
		{
			Log.Error( "There is no active dialogue set, unable to switch active labels!" );
			return;
		}

		SetLabel( _activeDialogue.Labels[afterLabel.TargetLabel] );
	}

	private void EndDialogue()
	{
		if ( ActiveScript is null || ActiveLabel is null )
		{
			Log.Warning( "Unable to end the active dialogue; there is none!" );
			return;
		}

		DialogueFinished = true;

		if ( ActiveScript is not null )
		{
			State.Choices = ActiveLabel.Choices;
		}
	}
}
