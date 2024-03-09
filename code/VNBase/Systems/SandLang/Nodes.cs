using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SandLang;

public class UndefinedVariable : Exception
{
	public UndefinedVariable( string name ) : base( $"Failed to find variable {name}!" )
	{
		base.Data["Missing Variable"] = name;
	}
}

public class InvalidParameters : Exception
{
	public InvalidParameters( IEnumerable<Value> values ) : base( "Invalid parameter types!" )
	{
		base.Data["Values"] = values;
	}
}

public interface IEnvironment
{
	public Value GetVariable( string name );
	public void SetVariable( string name, Value value );
	public IEnumerable<string> VariableSet();
}

public class EnvironmentMap : IEnvironment
{
	private readonly Dictionary<string, Value> _variables;

	public EnvironmentMap( Dictionary<string, Value> variables )
	{
		_variables = variables;
	}

	public EnvironmentMap() : this( new Dictionary<string, Value>() )
	{
	}

	public override string ToString()
	{
		return VariableSet().Select( k => '"' + k + '"' + " = " + GetVariable( k ) )
			.Aggregate( ( acc, v ) => acc + '\n' + v ).Trim();
	}

	public EnvironmentMap( IEnvironment copy ) : this()
	{
		foreach ( var key in copy.VariableSet() )
		{
			this.SetVariable( key, copy.GetVariable( key ) );
		}
	}

	public Value GetVariable( string name )
	{
		return _variables[name];
	}

	public void SetVariable( string name, Value value )
	{
		_variables[name] = value;
	}

	public IEnumerable<string> VariableSet()
	{
		return _variables.Keys;
	}
}

public class SParen : IReadOnlyList<Value>
{
	public abstract record Token
	{
		public record OpenParen : Token;

		public record Symbol( string Name ) : Token;

		public record String( string Text ) : Token;

		public record Number( string Value ) : Token;

		public record CloseParen : Token;
	}

	private readonly List<Value> _backingList;

	private SParen( List<Value> backingList )
	{
		_backingList = backingList;
	}

	public IEnumerator<Value> GetEnumerator()
	{
		return _backingList.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_backingList).GetEnumerator();
	}

	public int Count => _backingList.Count;

	public Value this[ int index ] => _backingList[index];

	private static IEnumerable<Token> TokenizeText( string text )
	{
		var symbolStart = 0;
		var isInQuote = false;

		for ( var i = 0; i < text.Length; i++ )
		{
			if ( isInQuote )
			{
				if ( text[i] != '"' ) continue;
				if ( text[i] == '"' )
				{
					yield return new Token.String( text.Substring( symbolStart, i - symbolStart + 1 ) );
					symbolStart = i + 1;
					isInQuote = false;
				}
			}
			else
			{
				if ( char.IsWhiteSpace( text[i] ) )
				{
					if ( i != symbolStart )
					{
						var sym = text.Substring( symbolStart, i - symbolStart );
						if ( sym.All( IsFloatChar ) )
						{
							yield return new Token.Number( sym );
						}
						else yield return new Token.Symbol( sym );
					}

					symbolStart = i + 1;
					continue;
				}
				else if ( text[i] == '"' )
				{
					isInQuote = true;
				}
			}

			if ( symbolStart != i && IsValidSymbolName( text[symbolStart] ) && !IsValidSymbolName( text[i] ) )
			{
				var sym = text.Substring( symbolStart, i - symbolStart );
				if ( sym.All( IsFloatChar ) )
				{
					yield return new Token.Number( sym );
				}
				else yield return new Token.Symbol( sym );

				symbolStart = i + 1;
			}

			if ( text[i] == '(' )
			{
				yield return new Token.OpenParen();
				symbolStart = i + 1;
			}

			if ( text[i] == ')' )
			{
				yield return new Token.CloseParen();
				symbolStart = i + 1;
			}
		}
	}

	private static bool IsFloatChar( char c )
	{
		return char.IsDigit( c ) || c is '.' or '-';
	}

	private static bool IsValidSymbolName( char symChar )
	{
		return char.IsLetterOrDigit( symChar ) || symChar is '-' or '+' or '/' or '*' or '.';
	}

	public static IEnumerable<SParen> ParseText( string text )
	{
		var tokenList = TokenizeText( text ).ToList();

		foreach ( var p in ProcessTokens( tokenList ) ) yield return p;
	}

	private static IEnumerable<SParen> ProcessTokens( List<Token> tokenList )
	{
		SParen currentParen = null;
		var tokenDepth = 0;
		for ( var tokenIndex = 0; tokenIndex < tokenList.Count; tokenIndex++ )
		{
			var token = tokenList[tokenIndex];
			switch ( token )
			{
				case Token.CloseParen:
					tokenDepth--;
					if ( tokenDepth == 0 && currentParen != null )
					{
						yield return currentParen;
						currentParen = null;
					}

					break;
				case Token.OpenParen:
					tokenDepth++;
					if ( tokenDepth == 1 )
					{
						currentParen = new SParen( new List<Value>() );
					}
					else
					{
						var subDepth = 1;
						var subToken = tokenIndex + 1;
						for ( ; subToken < tokenList.Count; subToken++ )
						{
							subDepth = tokenList[subToken] switch
							{
								Token.CloseParen => subDepth - 1,
								Token.OpenParen => subDepth + 1,
								_ => subDepth
							};
							if ( subDepth == 0 ) break;
						}

						foreach ( var sub in ProcessTokens(
							         tokenList.GetRange( tokenIndex, subToken - tokenIndex + 1 ) ) )
						{
							currentParen!._backingList.Add( new Value.ListValue( sub ) );
						}

						tokenDepth--;

						tokenIndex = subToken;
					}

					break;
				case Token.Number number:
					currentParen!._backingList.Add( new Value.NumberValue( decimal.Parse( number.Value ) ) );
					break;
				case Token.String str:
					currentParen!._backingList.Add(
						new Value.StringValue( str.Text.Substring( 1, str.Text.Length - 2 ) ) );
					break;
				case Token.Symbol symbol:
					currentParen!._backingList.Add( new Value.VariableReferenceValue( symbol.Name ) );
					break;
			}
		}
	}

	public Value Execute( IEnvironment environment )
	{
		return new Value.ListValue( this ).Evaluate( environment );
	}
}
