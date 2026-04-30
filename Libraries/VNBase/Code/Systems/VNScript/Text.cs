using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VNScript;

/// <summary>
/// Represents text that can be formatted with variables and expressions.
/// </summary>
public sealed class FormattableText( string text ) : IEquatable<string>
{
	public string Text { get; set; } = text;
	
	// Stores expressions that need to be evaluated at runtime
	// Key: placeholder index (e.g., "__EXPR_0__"), Value: the expression to evaluate
	private Dictionary<string, SParen> Expressions { get; } = new();
	
	private int _expressionCounter;
	
	/// <summary>
	/// Adds an expression placeholder to the text and stores the expression.
	/// Returns the placeholder string that was added.
	/// </summary>
	public string AddExpression( SParen expression )
	{
		var placeholder = $"__EXPR_{_expressionCounter}__";
		Expressions[placeholder] = expression;
		_expressionCounter++;
		
		return placeholder;
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
	/// Parses a raw string, extracting ${...} expressions into stored SParen entries.
	/// </summary>
	private static FormattableText Parse( string rawText )
	{
		var result = new FormattableText( string.Empty );
		var sb = new System.Text.StringBuilder();
		var i = 0;
		
		while ( i < rawText.Length )
		{
			if ( rawText[i] == '$' && i + 1 < rawText.Length && rawText[i + 1] == '{' )
			{
				var depth = 1;
				var j = i + 2;
				
				while ( j < rawText.Length && depth > 0 )
				{
					if ( rawText[j] == '{' )
					{
						depth++;
					}
					else if ( rawText[j] == '}' )
					{
						depth--;
					}
					if ( depth > 0 )
					{
						j++;
					}
				}
				
				var exprText = rawText[(i + 2)..j];
				var sparens = SParen.ParseText( $"({exprText})" ).ToList();
				
				if ( sparens.Count > 0 )
				{
					var placeholder = result.AddExpression( sparens[0] );
					sb.Append( "${" ).Append( placeholder ).Append( '}' );
				}
				else
				{
					sb.Append( "[Invalid: " ).Append( exprText ).Append( ']' );
				}
				
				i = j + 1;
			}
			else
			{
				sb.Append( rawText[i] );
				i++;
			}
		}
		
		result.Text = sb.ToString();
		return result;
	}
	
	/// <summary>
	/// Formats the text using the given environment.
	/// </summary>
	/// <param name="environment">The environment to format the text with.</param>
	/// <returns>The formatted text.</returns>
	public string Format( IEnvironment environment )
	{
		var result = Text;
		
		// Replace expression placeholders
		foreach ( var (placeholder, expression) in Expressions )
		{
			try
			{
				var value = expression.Execute( environment );
				result = result.Replace( $"${{{placeholder}}}", value.ToString() );
			}
			catch
			{
				result = result.Replace( $"${{{placeholder}}}", "[Error]" );
			}
		}
		
		// Replace variable placeholders
#pragma warning disable SYSLIB1045
		result = Regex.Replace( result, @"\$\{([^\{\}]+)\}", match =>
#pragma warning restore SYSLIB1045
		{
			var variableName = match.Groups[1].Value.Trim();
			
			if ( variableName.StartsWith( "__EXPR_" ) )
			{
				return match.Value;
			}
			
			var value = GetVariableValue( environment, variableName );
			return value.ToString();
		} );
		
		return result;
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
	public static implicit operator FormattableText( string text ) => Parse( text );
}
