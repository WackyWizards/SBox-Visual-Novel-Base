using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using VNBase;
using VNBase.Assets;

namespace VNScript;

/// <summary>
/// This class contains the dialogue structures as well as the functions to process dialogue and labels from the S-expression code
/// </summary>
public partial class Script
{
	public Dictionary<string, Label> Labels { get; } = new();

	public Label InitialLabel { get; private set; } = new();

	internal Dictionary<Value, Value> Variables { get; } = new();

	private static readonly Logger Log = new( "VNScript" );

	/// <summary>
	/// Parse a new script from the provided code.
	/// </summary>
	public static Script ParseScript( List<SParen> codeBlocks )
	{
		var script = new Script();
		script.Parse( codeBlocks );

		return script;
	}

	private void Parse( List<SParen> codeBlocks )
	{
		var parsingFunctions = CreateParsingFunctions();

		foreach ( var sParen in codeBlocks )
		{
			sParen.Execute( parsingFunctions );
		}
	}

	private EnvironmentMap CreateParsingFunctions()
	{
		var functionEnvironment = new EnvironmentMap();
		var functions = new Dictionary<string, Value.FunctionValue>
		{
			{
				"label", new Value.FunctionValue( CreateLabel )
			},
			{
				"start", new Value.FunctionValue( SetStartDialogue )
			},
			{
				"set", new Value.FunctionValue( SetVariable )
			},
		};

		foreach ( var function in functions )
		{
			functionEnvironment.SetVariable( function.Key, function.Value );
		}

		return functionEnvironment;
	}

	private Value.NoneValue SetVariable( IEnvironment environment, Value[] values )
	{
		for ( var i = 0; i < values.Length - 1; i += 2 )
		{
			var key = values[i];
			var value = values[i + 1];
			Variables[key] = value;
		}

		return Value.NoneValue.None;
	}

	private Value.NoneValue SetStartDialogue( IEnvironment environment, Value[] values )
	{
		InitialLabel = Labels[(values[0] as Value.VariableReferenceValue)!.Name];
		return Value.NoneValue.None;
	}

	private Value.NoneValue CreateLabel( IEnvironment environment, Value[] values )
	{
		var label = new Label();
		var labelName = values[0] switch
		{
			Value.StringValue stringValue => stringValue.Text,
			Value.VariableReferenceValue variableReferenceValue => variableReferenceValue.Name,
			_ => throw new InvalidParametersException( [values[0]] )
		};
		Labels[labelName] = label;
		label.Name = labelName;

		for ( var i = 1; i < values.Length; i++ )
		{
			var argument = ((Value.ListValue)values[i]).ValueList;
			ProcessLabelArgument( argument, label );
		}

		return Value.NoneValue.None;
	}

	private static void ProcessLabelArgument( SParen arguments, Label label )
	{
		var argumentType = ((Value.VariableReferenceValue)arguments[0]).Name;

		LabelArgument labelArgument = argumentType switch
		{
			"dialogue" =>
				LabelDialogueArgument,
			"choice" =>
				LabelChoiceArgument,
			"char" =>
				LabelCharacterArgument,
			"sound" =>
				LabelSoundArgument,
			"music" =>
				LabelMusicArgument,
			"bg" =>
				LabelBackgroundArgument,
			"input" =>
				LabelInputArgument,
			"after" =>
				LabelAfterArgument,
			_ => throw new ArgumentOutOfRangeException( argumentType )
		};

		labelArgument( arguments, label );
	}

	private delegate void LabelArgument( SParen argument, Label label );

	private delegate int DialogueArgument( SParen argument, int index, Label label, Dialogue dialogue );

	private delegate int ChoiceArgument( SParen argument, int index, Choice choice );

	private delegate int CharacterArgument( SParen argument, int index, Label label, Character character );

	private delegate int SoundArgument( SParen argument, int index, Label label, VNBase.Assets.Sound sound );

	private delegate int AfterArgument( SParen argument, int index, After after );

	private static void LabelAfterArgument( SParen arguments, Label label )
	{
		label.AfterLabel = new After();

		for ( var i = 1; i < arguments.Count; i++ )
		{
			switch ( arguments[i] )
			{
				case Value.ListValue listValue:
					label.AfterLabel.CodeBlocks.Add( listValue.ValueList );
					break;
				case Value.VariableReferenceValue variableReferenceValue:
					AfterArgument afterArgument = variableReferenceValue.Name switch
					{
						"end" => AfterEndDialogueArgument,
						"jump" => AfterJumpArgument,
						"load" => AfterLoadScriptArgument,
						_ => throw new ArgumentOutOfRangeException( variableReferenceValue.Name )
					};
					i += afterArgument( arguments, i, label.AfterLabel );
					break;
				default:
					throw new InvalidParametersException( [arguments[i]] );
			}
		}
	}

	private static int AfterJumpArgument( SParen arguments, int index, After after )
	{
		var labelName = (arguments[index + 1] as Value.VariableReferenceValue)!.Name;
		after.TargetLabel = labelName;
		return 1;
	}

	private static int AfterEndDialogueArgument( SParen arguments, int index, After after )
	{
		after.IsLastLabel = true;
		return 0;
	}

	private static int AfterLoadScriptArgument( SParen arguments, int index, After after )
	{
		after.ScriptPath = (arguments[index + 1] as Value.VariableReferenceValue)!.Name;
		return 1;
	}

	private static void LabelChoiceArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.StringValue argument )
		{
			throw new InvalidParametersException( [arguments[1]] );
		}

		var choice = new Choice();
		label.Choices.Add( choice );
		choice.Text = argument.Text;

		for ( var i = 2; i < arguments.Count; i++ )
		{
			if ( arguments[i] is not Value.VariableReferenceValue variableReferenceValue )
			{
				throw new InvalidParametersException( [arguments[i]] );
			}

			ChoiceArgument choiceArgument = variableReferenceValue.Name switch
			{
				"jump" => ChoiceJumpArgument,
				"cond" => ChoiceConditionArgument,
				_ => throw new ArgumentOutOfRangeException( variableReferenceValue.Name )
			};

			i += choiceArgument( arguments, i, choice );
		}
	}

	private static int ChoiceConditionArgument( SParen arguments, int index, Choice choice )
	{
		choice.Condition = (arguments[index + 1] as Value.ListValue)!.ValueList;
		return 1;
	}

	private static int ChoiceJumpArgument( SParen arguments, int index, Choice choice )
	{
		choice.TargetLabel = (arguments[index + 1] as Value.VariableReferenceValue)!.Name;
		return 1;
	}

	private static void LabelDialogueArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.StringValue argument )
		{
			throw new InvalidParametersException( [arguments[1]] );
		}

		var entry = new Dialogue
		{
			Text = new FormattableText( argument.Text ),
			Speaker = null
		};

		for ( var i = 2; i < arguments.Count; i++ )
		{
			if ( arguments[i] is not Value.VariableReferenceValue variableReferenceValue )
			{
				throw new InvalidParametersException( [arguments[i]] );
			}

			DialogueArgument dialogueArgument = variableReferenceValue.Name switch
			{
				"speaker" => DialogueSpeakerArgument,
				_ => throw new ArgumentOutOfRangeException( variableReferenceValue.Name )
			};

			i += dialogueArgument( arguments, i, label, entry );
		}
		
		label.Dialogues.Add( entry );
	}

	private static int DialogueSpeakerArgument( SParen arguments, int index, Label label, Dialogue dialogue )
	{
		var characterName = ((Value.VariableReferenceValue)arguments[3]).Name;
		var character = GetCharacterResource( characterName ) ?? throw new ResourceNotFoundException( $"Unable to set speaking character, character resource with name {characterName} couldn't be found!", characterName );
		dialogue.Speaker = character;

		return 1;
	}

	private static void LabelCharacterArgument( SParen arguments, Label label )
	{
		var characterName = ((Value.VariableReferenceValue)arguments[1]).Name;
		var character = GetCharacterResource( characterName ) ?? throw new ResourceNotFoundException( $"Unable to add character, character resource with name {characterName} couldn't be found!", characterName );
		label.Characters.Add( character );

		for ( var i = 2; i < arguments.Count; i++ )
		{
			if ( arguments[i] is not Value.VariableReferenceValue variableReferenceValue )
			{
				throw new InvalidParametersException( [arguments[i]] );
			}

			CharacterArgument characterArgument = variableReferenceValue.Name switch
			{
				"exp" => LabelCharacterExpressionArgument,
				_ => throw new ArgumentOutOfRangeException( variableReferenceValue.Name )
			};

			i += characterArgument( arguments, i, label, character );
		}
	}

	private static int LabelCharacterExpressionArgument( SParen arguments, int index, Label label, Character character )
	{
		if ( arguments[3] is not Value.VariableReferenceValue argument )
		{
			throw new InvalidParametersException( [arguments[3]] );
		}

		character.ActivePortrait = argument.Name;
		return 1;
	}

	private static void LabelSoundArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.StringValue argument )
		{
			throw new InvalidParametersException( [arguments[1]] );
		}

		var soundName = argument.Text;
		var sound = new VNBase.Assets.Sound( soundName );
		label.Assets.Add( sound );

		for ( var i = 2; i < arguments.Count; i++ )
		{
			if ( arguments[i] is not Value.VariableReferenceValue variableReferenceValue )
			{
				throw new InvalidParametersException( [arguments[i]] );
			}

			SoundArgument soundArgument = variableReferenceValue.Name switch
			{
				"mixer" => SoundMixerArgument,
				_ => throw new ArgumentOutOfRangeException( variableReferenceValue.Name )
			};

			i += soundArgument( arguments, i, label, sound );
		}
	}

	private static int SoundMixerArgument( SParen arguments, int index, Label label, VNBase.Assets.Sound sound )
	{
		if ( arguments[index + 1] is not Value.StringValue argument )
		{
			throw new InvalidParametersException( [arguments[1]] );
		}

		sound.MixerName = argument.Text;
		return 1;
	}

	private static void LabelMusicArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.StringValue argument )
		{
			throw new InvalidParametersException( [arguments[1]] );
		}

		var musicName = argument.Text;
		label.Assets.Add( new Music( musicName ) );
	}

	private static void LabelBackgroundArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.StringValue argument )
		{
			throw new InvalidParametersException( [arguments[1]] );
		}

		var backgroundName = argument.Text;
		var backgroundPath = $"{Settings.BackgroundsPath}{backgroundName}";
		label.Assets.Add( new Background( backgroundPath ) );
	}

	private static void LabelInputArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.VariableReferenceValue argument )
		{
			throw new InvalidParametersException( [arguments[1]] );
		}

		if ( label.Choices.Count > 0 )
		{
			throw new InvalidOperationException( "Cannot have a text input in a label with choices!" );
		}

		label.ActiveInput = new Input
		{
			VariableName = argument.Name
		};
	}

	private static Character? GetCharacterResource( string characterName )
	{
		var characterPath = $"{Settings.CharacterResourcesPath}{characterName}.char";
		return ResourceLibrary.TryGet<Character>( characterPath, out var loadedCharacter ) ? loadedCharacter : null;
	}
}
