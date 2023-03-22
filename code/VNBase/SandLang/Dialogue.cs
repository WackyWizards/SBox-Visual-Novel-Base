using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VNBase;

namespace SandLang;

/// <summary>
/// This class contains the dialogue structures as well as the functions to process dialogue and labels from the S-expression code
/// </summary>
public class Dialogue
{
	/// <summary>
	/// Represents a choice by the player, possible required conditions for it to be a viable choice, and the new label to direct towards
	/// </summary>
    public class Choice
    {
        public string ChoiceText { get; set; }
        public string TargetLabel { get; set; }
        public SParen Condition { get; set; }
    }
	
	/// <summary>
	/// In labels that do not have choices, represents code to execute as well as the new label to direct towards
	/// </summary>
    public class AfterLabel
    {
        public List<SParen> CodeBlocks { get; set; } = new();
        public string TargetLabel { get; set; }
    }

	/// <summary>
	/// Represents a dialogue step by the other character + options for the player to take
	/// </summary>
    public class Label
    {
        public string Text { get; set; }
		public CharacterBase Character { get; set; }
		public List<Choice> Choices { get; set; }
        public AfterLabel AfterLabel { get; set; }
    }

    public Dictionary<string, Label> DialogueLabels { get; } = new();
    public Label InitialLabel { get; private set; }

    public static Dialogue ParseDialogue(List<SParen> codeBlocks)
    {
        var dialogue = new Dialogue();
        dialogue._parseDialogue(codeBlocks);
        return dialogue;
    }

	private void _parseDialogue( List<SParen> codeblocks )
	{
		var dialogueParsingFunctions = new EnvironmentMap();

		dialogueParsingFunctions.SetVariable( "label", new Value.FunctionValue( CreateLabel ) );
		dialogueParsingFunctions.SetVariable( "start-dialogue", new Value.FunctionValue( SetStartDialogue ) );
		dialogueParsingFunctions.SetVariable( "character", new Value.FunctionValue( CreateCharacter ) );

		foreach ( var sParen in codeblocks )
		{
			sParen.Execute( dialogueParsingFunctions );
		}
	}

	private Value SetStartDialogue(IEnvironment environment, Value[] values)
    {
        InitialLabel = DialogueLabels[(values[0] as Value.VariableReferenceValue)!.Name];
        return Value.NoneValue.None;
    }

    private Value CreateLabel(IEnvironment environment, Value[] values)
    {
        var label = new Label();
        var labelName = values[0] switch
        {
            Value.StringValue stringValue => stringValue.Text,
            Value.VariableReferenceValue variableReferenceValue => variableReferenceValue.Name,
            _ => throw new InvalidParameters(new[] { values[0] })
        };
        DialogueLabels[labelName] = label;

        for (var i = 1; i < values.Length; i++)
        {
            var argument = ((Value.ListValue)values[i]).ValueList;
            ProcessLabelArgument(argument, label);
        }

        return Value.NoneValue.None;
    }

	private Value CreateCharacter(IEnvironment environment, Value[] values)
	{
		string characterVariableName = ((Value.StringValue)values[0]).Text;

		// Create a new Label for the character
		var label = new Label();
		DialogueLabels[characterVariableName] = label;

		CharacterBase character = CreateCharacter(characterVariableName);

		// Process the character arguments
		for ( var i = 1; i < values.Length; i++ )
		{
			var argument = ((Value.ListValue)values[i]).ValueList;
			ProcessLabelArgument( argument, label );
		}

		// Return a wrapper value containing the character object
		return new Value.WrapperValue<CharacterBase>( character );
	}

	private static void ProcessLabelArgument(SParen argument, Label label)
    {
        var argumentType = ((Value.VariableReferenceValue)argument[0]).Name;
        LabelArgument labelArgument = (argumentType switch
        {
            "text" =>
                LabelTextArgument,
            "choice" =>
                LabelChoiceArgument,
            "after" =>
                LabelAfterArgument,
			"character" =>
				LabelCharacterArgument,
            _ => throw new ArgumentOutOfRangeException()
        });
        labelArgument(argument, label);
    }

    private delegate void LabelArgument(SParen argument, Label label);

    /// <summary>
    /// Returns consumed values in argument list
    /// </summary>
    private delegate int ChoiceArgument(SParen argument, int index, Choice choice);

    /// <summary>
    /// Returns consumed values in argument list
    /// </summary>
    private delegate int AfterArgument(SParen argument, int index, AfterLabel after);

	private delegate void CharacterArgument(SParen argument, Label label);

	private static void LabelAfterArgument(SParen argument, Label label)
    {
        label.AfterLabel = new AfterLabel();

        for (var i = 1; i < argument.Count; i++)
        {
            switch (argument[i])
            {
                case Value.ListValue listValue:
                    label.AfterLabel.CodeBlocks.Add(listValue.ValueList);
                    break;
                case Value.VariableReferenceValue variableReferenceValue:
                    AfterArgument afterArgument = variableReferenceValue.Name switch
                    {
                        "end-dialogue" => AfterEndDialogueArgument,
                        "jump" => AfterJumpArgument,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    i += afterArgument(argument, i, label.AfterLabel);
                    break;
                default:
                    throw new InvalidParameters(new[] { argument[i] });
            }
        }
    }

    private static int AfterJumpArgument(SParen paren, int index, AfterLabel after)
    {
        after.TargetLabel = (paren[index + 1] as Value.VariableReferenceValue)!.Name;
        return 1;
    }

    private static int AfterEndDialogueArgument(SParen sParen, int i, AfterLabel after)
    {
        after.TargetLabel = null;
        return 0;
    }

    private static void LabelChoiceArgument(SParen argument, Label label)
    {
        label.Choices ??= new List<Choice>();
        var choice = new Choice();
        label.Choices.Add(choice);
        if (argument[1] is not Value.StringValue s) throw new InvalidParameters(new[] { argument[1] });
        choice.ChoiceText = s.Text;

        for (var i = 2; i < argument.Count; i++)
        {
            if (argument[i] is not Value.VariableReferenceValue variableReferenceValue)
            {
                throw new InvalidParameters(new[] { argument[i] });
            }

            ChoiceArgument choiceArgument = variableReferenceValue.Name switch
            {
                "jump" => ChoiceJumpArgument,
                "cond" => ChoiceConditionArgument,
                _ => throw new ArgumentOutOfRangeException()
            };

            i += choiceArgument(argument, i, choice);
        }
    }

    private static int ChoiceConditionArgument(SParen argument, int index, Choice choice)
    {
        choice.Condition = (argument[index + 1] as Value.ListValue)!.ValueList;
        return 1;
    }

    private static int ChoiceJumpArgument(SParen arg, int index, Choice ch)
    {
        ch.TargetLabel = (arg[index + 1] as Value.VariableReferenceValue)!.Name;
        return 1;
    }

    private static void LabelTextArgument(SParen argument, Label label)
    {
        if (argument[1] is not Value.StringValue s) throw new InvalidParameters(new[] { argument[1] });
        label.Text = s.Text;
    }

	private static void LabelCharacterArgument(SParen argument, Label label)
	{
		string characterName = ((Value.VariableReferenceValue)argument[1]).Name;
		CharacterBase character = CreateCharacter(characterName);
		label.Character = character;
	}

	private static CharacterBase CreateCharacter(string characterName)
	{
		var characterType = TypeLibrary.GetType( characterName )?.TargetType;
		if ( characterType == null )
		{
			throw new ArgumentException( $"Can't find character with name {characterName}!" );
		}

		var character = TypeLibrary.Create<CharacterBase>( characterType );
		return character;
	}
}
