using System;
using System.Text.RegularExpressions;

namespace VNScript;

/// <summary>
/// Represents text that can be formatted.
/// </summary>
public sealed class FormattableText( string text ) : IEquatable<string>
{
	// ReSharper disable once MemberCanBePrivate.Global
	public string Text { get; set; } = text;

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
    public string Format( IEnvironment environment )
    {
#pragma warning disable SYSLIB1045
	    return Regex.Replace( Text, @"\{(\w+)\}", match =>
#pragma warning restore SYSLIB1045
	    {
            var variableName = match.Groups[1].Value;
            return GetVariableValue( environment, variableName ).ToString();
        } );
    }

    private static Value GetVariableValue( IEnvironment environment, string variableName )
    {
        try 
        {
            return environment.GetVariable( variableName );
        }
        catch
        {
            return Value.NoneValue.None;
        }
    }

    public static implicit operator string( FormattableText formattableText ) => formattableText.Text;
    public static implicit operator FormattableText( string text ) => new( text );
}
