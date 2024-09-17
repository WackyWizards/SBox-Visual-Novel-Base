using System;
using System.Text.RegularExpressions;

namespace SandLang;

/// <summary>
/// Represents text that can be formatted.
/// </summary>
public class FormattableText : IEquatable<string>
{
    public string Text { get; set; }

    public FormattableText( string text )
    {
        Text = text;
    }

    public bool Equals( string? other )
    {
        return Text == other;
    }

    public override string ToString()
    {
        return Text;
    }

    /// <summary>
    /// Formats the text using the given environment.
    /// </summary>
    /// <param name="environment">The environment to format the text with.</param>
    /// <returns>The formatted text.</returns>
    public virtual string Format( IEnvironment environment )
    {
        return Regex.Replace( Text, "\\{(\\w+)\\}", match =>
        {
            var variableName = match.Groups[1].Value;
            try
            {
                var value = environment.GetVariable( variableName );
                return value.ToString();
            }
            catch ( UndefinedVariableException )
            {
                return string.Empty;
            }
        } );
    }

    public static implicit operator string( FormattableText formattableText ) => formattableText.Text;
    public static implicit operator FormattableText( string text ) => new( text );
}
