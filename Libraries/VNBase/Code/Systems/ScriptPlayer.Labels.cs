using Sandbox;
using System;
using System.Linq;
using SandLang;
using VNBase.UI;
using VNBase.Assets;

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

		foreach ( Assets.Sound sound in label.Assets.OfType<Assets.Sound>() )
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

		_cts = new();

		IEnvironment environment = _environment;
		string formattedText = label.Text.Format( environment );
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
		DialogueFinished = true;

		if ( ActiveScript is not null )
		{
			State.Choices = label.Choices;
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

		if ( afterLabel is not null )
		{
			foreach ( var codeBlock in afterLabel.CodeBlocks )
			{
				codeBlock.Execute( ActiveScript.GetEnvironment() );
			}

			bool hasInput = ActiveLabel.ActiveInput is not null;
			if ( hasInput && Hud is not null )
			{
				var input = Hud.GetSubPanel<TextInput>();

				if ( input is not null && string.IsNullOrWhiteSpace( input.Entry.Text ) )
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

			if ( afterLabel.TargetLabel is not null )
			{
				if ( _activeDialogue is null )
				{
					Log.Error( "There is no active dialogue set, unable to switch active labels!" );
					return;
				}

				SetLabel( _activeDialogue.Labels[afterLabel.TargetLabel] );
			}
		}
	}

}
