using System;
using System.Collections.Generic;
using System.Linq;

namespace SandLang;

internal static class BuiltinFunctions
{
	/// <summary>
	/// Contains mappings from symbols to builtin executable functions
	/// </summary>
	public static Dictionary<string, Value.FunctionValue> Builtins { get; } = new()
	{
		["="] = new Value.FunctionValue( EqualityFunction ),
		[">"] = new Value.FunctionValue( GreaterThanFunction ),
		["<"] = new Value.FunctionValue( LessThanFunction ),
		["+"] = new Value.FunctionValue( SumFunction ),
		["-"] = new Value.FunctionValue( SubtractFunction ),
		["*"] = new Value.FunctionValue( MulFunction ),
		["set"] = new Value.FunctionValue( SetFunction ),
		["defun"] = new Value.FunctionValue( DefineFunction ),
		["pow"] = new Value.FunctionValue( PowFunction ),
		["sqrt"] = new Value.FunctionValue( SqrtFunction ),
		["if"] = new Value.FunctionValue( IfFunction ),
		["body"] = new Value.FunctionValue( ExpressionBodyFunction ),
	};

	private static Value GreaterThanFunction( IEnvironment environment, Value[] values )
	{
		var v1 = values[0].Evaluate( environment ) as Value.NumberValue;
		var v2 = values[1].Evaluate( environment ) as Value.NumberValue;

		return new Value.BooleanValue( v1!.Number > v2!.Number );
	}

	private static Value LessThanFunction( IEnvironment environment, Value[] values )
	{
		var v1 = values[0].Evaluate( environment ) as Value.NumberValue;
		var v2 = values[1].Evaluate( environment ) as Value.NumberValue;

		return new Value.BooleanValue( v1!.Number < v2!.Number );
	}

	private static Value EqualityFunction( IEnvironment environment, Value[] values )
	{
		var v1 = values[0].Evaluate( environment );
		var v2 = values[1].Evaluate( environment );

		if ( v1.GetType() != v2.GetType() )
		{
			throw new InvalidParametersException( new[] { v1, v2 } );
		}

		if ( v2 is Value.BooleanValue booleanValue )
		{
			return new Value.BooleanValue( ((Value.BooleanValue)v1).Boolean.Equals( booleanValue.Boolean ) );
		}
		else if ( v2 is Value.NumberValue number )
		{
			return new Value.BooleanValue( ((Value.NumberValue)v1).Number.Equals( number.Number ) );
		}

		return Value.NoneValue.None;
	}

	private static Value ExpressionBodyFunction( IEnvironment environment, Value[] values )
	{
		Value lastValue = Value.NoneValue.None;

		foreach ( var expression in values )
		{
			lastValue = expression.Evaluate( environment );
		}

		return lastValue;
	}

	private static Value IfFunction( IEnvironment environment, Value[] values )
	{
		var cond = values[0].Evaluate( environment ) as Value.NumberValue;
		if ( cond!.Number < 0 )
		{
			if ( values.Length == 1 )
			{
				throw new InvalidParametersException( values );
			}

			return values[1].Evaluate( environment );
		}

		if ( values.Length > 2 )
		{
			return values[2].Evaluate( environment );
		}

		return Value.NoneValue.None;
	}

	private static Value MulFunction( IEnvironment environment, Value[] values )
	{
		return new Value.NumberValue( values.Select( p => p.Evaluate( environment ) ).Select( p => p switch
		{
			Value.NumberValue numberValue => numberValue.Number,
			_ => throw new InvalidParametersException( new[] { p } )
		} ).Aggregate( ( acc, v ) => acc + v ) );
	}

	private static Value PowFunction( IEnvironment environment, Value[] values )
	{
		var baseVal = values[0].Evaluate( environment ) as Value.NumberValue;
		var exponentVal = values[1].Evaluate( environment ) as Value.NumberValue;
		return new Value.NumberValue( new decimal( Math.Pow( (double)baseVal!.Number, (double)exponentVal!.Number ) ) );
	}

	private static Value SqrtFunction( IEnvironment environment, Value[] values )
	{
		var val = values[0].Evaluate( environment ) as Value.NumberValue;
		return new Value.NumberValue( new decimal( Math.Sqrt( (double)val!.Number ) ) );
	}

	private static Value DefineFunction( IEnvironment environment, Value[] values )
	{
		var argnames = (values[0] as Value.ListValue)!.ValueList.Select( p => p switch
		{
			Value.StringValue stringValue => stringValue.Text,
			Value.VariableReferenceValue variableReferenceValue => variableReferenceValue.Name,
			_ => throw new ArgumentOutOfRangeException( nameof( p ) )
		} ).ToArray();

		var body = (values[1] as Value.ListValue)!.ValueList;

		return new Value.FunctionValue( ( _, arglist ) =>
		{
			var env = new EnvironmentMap( environment );

			for ( var i = 0; i < MathF.Min( argnames.Length, arglist.Length ); i++ )
			{
				env.SetVariable( argnames[i], arglist[i].Evaluate( _ ) );
			}

			return body.Execute( env );
		} );
	}

	private static Value SetFunction( IEnvironment environment, params Value[] values )
	{
		if ( values.Length != 2 ) throw new InvalidParametersException( values );
		var varname = values[0];

		if ( varname is not Value.VariableReferenceValue vrv )
		{
			throw new InvalidParametersException( new[] { varname } );
		}

		var value = values[1].Evaluate( environment );
		environment.SetVariable( vrv.Name, value );

		return value;
	}

	private static Value SubtractFunction( IEnvironment environment, params Value[] values )
	{
		values = values.Select( v => v.Evaluate( environment ) ).ToArray();

		if ( !values.All( v => v is Value.NumberValue ) )
		{
			throw new InvalidParametersException( values );
		}

		return values[0] switch
		{
			Value.NumberValue => new Value.NumberValue( values.Select( v => (v as Value.NumberValue)!.Number )
				.Aggregate( ( acc, v ) => acc - v ) ),
			_ => Value.NoneValue.None,
		};
	}

	private static Value SumFunction( IEnvironment environment, params Value[] values )
	{
		values = values.Select( v => v.Evaluate( environment ) ).ToArray();

		if ( !(values.All( v => v is Value.StringValue ) || values.All( v => v is Value.NumberValue )) )
		{
			throw new InvalidParametersException( values );
		}

		return values[0] switch
		{
			Value.StringValue => new Value.StringValue(
				values.Select( v => (v as Value.StringValue)!.Text ).Aggregate( ( acc, v ) => acc + v ) ),
			Value.NumberValue =>
				new Value.NumberValue( values.Select( v => (v as Value.NumberValue)!.Number ).Sum() ),
			_ => Value.NoneValue.None
		};
	}
}
