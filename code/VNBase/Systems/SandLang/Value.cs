using System.Linq;

namespace SandLang;

public abstract record Value
{
	public virtual Value Evaluate( IEnvironment environment )
	{
		return this;
	}

	public delegate Value FunctionMapping( IEnvironment environment, params Value[] values );

	/// <summary>
	/// Not really in use right now, but represents the equivalent of a null in LISP
	/// </summary>
	public record NoneValue : Value
	{
		private NoneValue() { }
		public static readonly NoneValue None = new();
	}

	/// <summary>
	/// For representing C# world variables (such as entities) in LISP (cannot be used with builtins, only really
	/// useful for environment-defined functions)
	/// </summary>
	public record WrapperValue<T>( T Value ) : Value;

	/// <summary>
	/// Represents symbols, that is anything that isn't a number or a string and hasn't been dereferenced to a variable yet
	/// </summary>
	public record VariableReferenceValue( string Name ) : Value
	{
		public override Value Evaluate( IEnvironment environment )
		{
			if ( environment.VariableSet().Contains( Name ) )
				return environment.GetVariable( Name );

			if ( BuiltinFunctions.Builtins.TryGetValue( Name, out var builtin ) )
			{
				return builtin;
			}

			throw new UndefinedVariable( Name );
		}
	}

	public record StringValue( string Text ) : Value;

	public record NumberValue( decimal Number ) : Value;

	public record FunctionValue( FunctionMapping Function ) : Value;

	public record ListValue( SParen ValueList ) : Value
	{
		public override Value Evaluate( IEnvironment environment )
		{
			if ( ValueList.Count <= 0 ) return this;
			var value0 = ValueList[0];
			if ( value0 is VariableReferenceValue variableReferenceValue )
			{
				value0 = variableReferenceValue.Evaluate( environment );
			}

			if ( value0 is FunctionValue functionValue )
			{
				return functionValue.Function( environment, ValueList.Skip( 1 ).ToArray() ) ?? NoneValue.None;
			}

			return this;
		}
	}
}
