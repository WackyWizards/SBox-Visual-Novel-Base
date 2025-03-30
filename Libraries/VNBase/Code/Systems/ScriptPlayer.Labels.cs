using System;
using System.Linq;
using System.Threading;
using Sandbox.Audio;
using SandLang;
using VNBase.UI;
using VNBase.Assets;
using Sound=VNBase.Assets.Sound;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	private async void SetLabel( Dialogue.Label label )
	{
		try
		{
			// Clean up any existing text effect before starting a new one
			SkipDialogueEffect();
			ActiveLabel = label;

			if ( LoggingEnabled )
			{
				Log.Info( $"Loading Label {label.Name}" );
			}

			State.Characters.Clear();
			label.Characters.ForEach( State.Characters.Add );
			State.SpeakingCharacter = label.SpeakingCharacter;

			foreach ( var sound in label.Assets.OfType<Sound>() )
			{
				State.Sounds.Add( sound );

				if ( string.IsNullOrEmpty( sound.MixerName ) )
				{
					sound.Play();
				}
				else
				{
					sound.Play( sound.MixerName );
				}

				if ( sound is Music && sound.Handle is not null )
				{
					sound.Handle.TargetMixer = Mixer.FindMixerByName( "Music" );
				}

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

			AddHistory( label );
			OnLabelSet?.Invoke( label );

			var formattedText = label.Text.Format( _environment );

			if ( Settings?.TextEffectEnabled ?? false )
			{
				_cts = new CancellationTokenSource();

				try
				{
					await Settings.TextEffect.Play( formattedText, (int)Settings.TextEffectSpeed, UpdateDialogueText, _cts.Token );
					EndDialogue( formattedText, label );
				}
				catch ( OperationCanceledException )
				{
					EndDialogue( formattedText, label );
				}
			}
			else
			{
				// Skip the text effect entirely
				EndDialogue( formattedText, label );
			}
		}
		catch ( Exception e )
		{
			Log.Error( e.Message );
		}
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

		foreach ( var sound in State.Sounds.Where( sound => sound is not Music ).ToArray() )
		{
			sound.Stop();
			State.Sounds.Remove( sound );
		}

		foreach ( var codeBlock in afterLabel.CodeBlocks )
		{
			codeBlock.Execute( ActiveScript.GetEnvironment() );
		}

		// Do not let us continue if there is an empty input box.
		var hasInput = ActiveLabel.ActiveInput is not null;
		if ( hasInput && Hud is not null )
		{
			var input = Hud.GetSubPanel<TextInput>();

			if ( input is null )
				return;

			if ( string.IsNullOrWhiteSpace( input.Entry?.Text ) )
			{
				return;
			}
		}

		if ( afterLabel.IsLastLabel )
		{
			UnloadScript();
			return;
		}

		if ( !string.IsNullOrEmpty( afterLabel.ScriptPath ) )
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

	private async void EndDialogue( string text, Dialogue.Label label )
	{
		try
		{
			if ( ActiveScript is null || ActiveLabel is null )
			{
				return;
			}

			// If we are in Automatic Mode, wait a bit before ending the dialogue.
			if ( IsAutomaticMode && label.Choices.Count == 0 )
			{
				try
				{
					await Task.DelaySeconds( Settings.AutoDelay );
				}
				catch ( OperationCanceledException )
				{
					State.IsDialogueFinished = false;
				}
			}

			State.DialogueText = text;
			State.Choices = ActiveLabel.Choices;
			State.IsDialogueFinished = true;
		}
		catch ( Exception e )
		{
			Log.Error( e.Message );
		}
	}

	private void UpdateDialogueText( string text )
	{
		State.DialogueText = text;
		State.IsDialogueFinished = false;
	}
}
