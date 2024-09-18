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

		if ( Game.IsEditor )
		{
			Log.Info( $"Loading Label {label.Name}" );
		}

		Characters.Clear();
		label.Characters.ForEach( Characters.Add );
		SpeakingCharacter = label.SpeakingCharacter;

		foreach ( SoundAsset sound in label.Assets.OfType<SoundAsset>() )
		{
			sound.Play();

			if ( Game.IsEditor )
			{
				Log.Info( $"Played SoundAsset {sound} from label {label.Name}" );
			}
		}

		try
		{
			Background = label.Assets.OfType<BackgroundAsset>().SingleOrDefault()?.Path;
		}
		catch ( InvalidOperationException )
		{
			Log.Error( $"There can only be one {nameof( BackgroundAsset )} in label {label.Name}!" );
			Background = null;
		}

		_cts = new();

		string formattedText = label.Text.Format( _environment ?? new EnvironmentMap() );
		if ( Settings?.TextEffectEnabled ?? false && Settings.TextEffect is not null )
		{
			try
			{
				await Settings.TextEffect.Play( formattedText, Settings.TextEffectDelay, ( text ) => DialogueText = text, _cts.Token );
			}
			catch ( OperationCanceledException )
			{
				DialogueText = formattedText;
			}
		}
		else
		{
			DialogueText = formattedText;
		}

		AddHistory( label );
		DialogueFinished = true;

		if ( ActiveScript is not null )
		{
			var environment = _environment ?? new EnvironmentMap();
			SetChoices( environment, label.Choices );
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
				LoadScript( Settings.ScriptPath + afterLabel.ScriptPath );
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