using Sandbox;
using System;
using System.Collections.Generic;
using VNBase;
using VNBase.Assets;

namespace SandLang;

/// <summary>
/// This class contains the dialogue structures as well as the functions to process dialogue and labels from the S-expression code
/// </summary>
public class Dialogue
{
	public Dictionary<string, Label> Labels { get; } = new();

	public Label InitialLabel { get; private set; } = new();

	public Dictionary<Value, Value> Variables { get; } = new();

	/// <summary>
	/// Represents a dialogue step.
	/// </summary>
	public class Label
	{
		public string Name { get; set; } = string.Empty;

		public string Text { get; set; } = string.Empty;

		public Character? SpeakingCharacter { get; set; }

		public List<Character> Characters { get; set; } = new();

		public List<Choice> Choices { get; set; } = new();

		public List<Asset> Assets { get; set; } = new();

		public AfterLabel? AfterLabel { get; set; }
	}

	/// <summary>
	/// Represents a choice by the player, possible required conditions for it to be a viable choice, and the new label to direct towards.
	/// </summary>
	public class Choice
	{
		public string ChoiceText { get; set; } = string.Empty;

		public string TargetLabel { get; set; } = string.Empty;

		public SParen? Condition { get; set; }

		/// <summary>
		/// Returns whether this condition is available to the player.
		/// </summary>
		public bool IsAvailable( IEnvironment environment )
		{
			if ( Condition == null )
				return true;

			var value = Condition.Execute( environment );

			if ( value is Value.BooleanValue boolValue )
			{
				return boolValue.Boolean;
			}

			return false;
		}
	}

	/// <summary>
	/// In labels that do not have choices, represents code to execute as well as the new label to direct towards.
	/// </summary>
	public class AfterLabel
	{
		public List<SParen> CodeBlocks { get; set; } = new();

		public bool IsLastLabel { get; set; }

		public string? TargetLabel { get; set; }
	}

	public static Dialogue ParseDialogue( List<SParen> codeBlocks )
	{
		var dialogue = new Dialogue();
		dialogue.Parse( codeBlocks );
		return dialogue;
	}

	private void Parse( List<SParen> codeBlocks )
	{
		var dialogueParsingFunctions = new EnvironmentMap();

		dialogueParsingFunctions.SetVariable( "label", new Value.FunctionValue( CreateLabel ) );
		dialogueParsingFunctions.SetVariable( "start-dialogue", new Value.FunctionValue( SetStartDialogue ) );
		dialogueParsingFunctions.SetVariable( "set", new Value.FunctionValue( SetVariable ) );

		foreach ( var sParen in codeBlocks )
		{
			sParen.Execute( dialogueParsingFunctions );
		}
	}

	private Value SetVariable( IEnvironment environment, Value[] values )
	{
		for ( int i = 0; i < values.Length - 1; i += 2 )
		{
			var key = values[i];
			var value = values[i + 1];
			Variables[key] = value;
		}

		return Value.NoneValue.None;
	}

	private Value SetStartDialogue( IEnvironment environment, Value[] values )
	{
		InitialLabel = Labels[(values[0] as Value.VariableReferenceValue)!.Name];
		return Value.NoneValue.None;
	}

	private Value CreateLabel( IEnvironment environment, Value[] values )
	{
		var label = new Label();
		var labelName = values[0] switch
		{
			Value.StringValue stringValue => stringValue.Text,
			Value.VariableReferenceValue variableReferenceValue => variableReferenceValue.Name,
			_ => throw new InvalidParametersException( new[] { values[0] } )
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
			"text" =>
				LabelTextArgument,
			"choice" =>
				LabelChoiceArgument,
			"character" =>
				LabelCharacterArgument,
			"sound" =>
				LabelSoundArgument,
			"bg" =>
				LabelBackgroundArgument,
			"after" =>
				LabelAfterArgument,
			_ => throw new ArgumentOutOfRangeException()
		};

		labelArgument( arguments, label );
	}

	private delegate void LabelArgument( SParen argument, Label label );

	private delegate int TextArgument( SParen argument, int index, Label label );

	private delegate int ChoiceArgument( SParen argument, int index, Choice choice );

	private delegate int CharacterArgument( SParen argument, int index, Label label, Character character );

	private delegate int SoundArgument( SParen argument, int index, Label label );

	private delegate int BackgroundArgument( SParen argument, int index, Label label );

	private delegate int AfterArgument( SParen argument, int index, AfterLabel after );

	private static void LabelAfterArgument( SParen arguments, Label label )
	{
		label.AfterLabel = new();

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
						"end-dialogue" => AfterEndDialogueArgument,
						"jump" => AfterJumpArgument,
						_ => throw new ArgumentOutOfRangeException()
					};
					i += afterArgument( arguments, i, label.AfterLabel );
					break;
				default:
					throw new InvalidParametersException( new[] { arguments[i] } );
			}
		}
	}

	private static int AfterJumpArgument( SParen arguments, int index, AfterLabel after )
	{
		string labelName = (arguments[index + 1] as Value.VariableReferenceValue)!.Name;
		after.TargetLabel = labelName;
		return 1;
	}

	private static int AfterEndDialogueArgument( SParen arguments, int index, AfterLabel after )
	{
		after.IsLastLabel = true;
		return 0;
	}

	private static void LabelChoiceArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.StringValue argument ) throw new InvalidParametersException( new[] { arguments[1] } );

		var choice = new Choice();
		label.Choices.Add( choice );
		choice.ChoiceText = argument.Text;

		for ( var i = 2; i < arguments.Count; i++ )
		{
			if ( arguments[i] is not Value.VariableReferenceValue variableReferenceValue )
			{
				throw new InvalidParametersException( new[] { arguments[i] } );
			}

			ChoiceArgument choiceArgument = variableReferenceValue.Name switch
			{
				"jump" => ChoiceJumpArgument,
				"cond" => ChoiceConditionArgument,
				_ => throw new ArgumentOutOfRangeException()
			};

			i += choiceArgument( arguments, i, choice );
		}
	}

	private static int ChoiceConditionArgument( SParen arguments, int index, Choice choice )
	{
		choice.Condition = (arguments[index + 1] as Value.ListValue)!.ValueList;
		return 1;
	}

	private static int ChoiceJumpArgument( SParen arguments, int index, Choice ch )
	{
		ch.TargetLabel = (arguments[index + 1] as Value.VariableReferenceValue)!.Name;
		return 1;
	}

	private static void LabelTextArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.StringValue argument ) throw new InvalidParametersException( new[] { arguments[1] } );
		label.Text = argument.Text;

		for ( var i = 2; i < arguments.Count; i++ )
		{
			if ( arguments[i] is not Value.VariableReferenceValue variableReferenceValue )
			{
				throw new InvalidParametersException( new[] { arguments[i] } );
			}

			TextArgument textArgument = variableReferenceValue.Name switch
			{
				"say" => TextSayArgument,
				_ => throw new NotImplementedException()
			};

			i += textArgument( arguments, i, label );
		}
	}

	private static int TextSayArgument( SParen arguments, int index, Label label )
	{
		string characterName = ((Value.VariableReferenceValue)arguments[3])!.Name;
		Character? character = GetCharacterResource( characterName ) ?? throw new ResourceNotFoundException( $"Unable to set speaking character, character resource with name {characterName} couldn't be found!", characterName );
		label.SpeakingCharacter = character;

		return 1;
	}

	private static void LabelCharacterArgument( SParen arguments, Label label )
	{
		string characterName = ((Value.VariableReferenceValue)arguments[1])!.Name;
		Character? character = GetCharacterResource( characterName ) ?? throw new ResourceNotFoundException( $"Unable to add character, character resource with name {characterName} couldn't be found!", characterName );
		label.Characters.Add( character );

		for ( var i = 2; i < arguments.Count; i++ )
		{
			if ( arguments[i] is not Value.VariableReferenceValue variableReferenceValue )
			{
				throw new InvalidParametersException( new[] { arguments[i] } );
			}

			CharacterArgument characterArgument = variableReferenceValue.Name switch
			{
				"exp" => LabelCharacterExpressionArgument,
				_ => throw new ArgumentOutOfRangeException()
			};

			i += characterArgument( arguments, i, label, character );
		}
	}

	private static int LabelCharacterExpressionArgument( SParen arguments, int index, Label label, Character character )
	{
		if ( arguments[3] is not Value.VariableReferenceValue argument ) throw new InvalidParametersException( new[] { arguments[3] } );
		character.ActivePortrait = argument.Name;
		return 1;
	}

	private static void LabelSoundArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.VariableReferenceValue argument ) throw new InvalidParametersException( new[] { arguments[1] } );

		string soundName = argument.Name;
		label.Assets.Add( new SoundAsset( soundName ) );
	}

	private static void LabelBackgroundArgument( SParen arguments, Label label )
	{
		if ( arguments[1] is not Value.VariableReferenceValue argument ) throw new InvalidParametersException( new[] { arguments[1] } );

		string backgroundName = argument.Name;
		label.Assets.Add( new BackgroundAsset( $"{Settings.BackgroundsPath}{backgroundName}" ) );
	}

	private static Character? GetCharacterResource( string characterName )
	{
		if ( ResourceLibrary.TryGet<Character>( $"{Settings.CharacterResourcesPath}{characterName}.char", out var loadedCharacter ) )
		{
			return loadedCharacter;
		}
		else
		{
			return null;
		}
	}
}
