using System;
using System.Linq;
using System.Collections.Generic;

namespace VNScript;

internal static class BuiltinFunctions
{
	/// <summary>
	/// Contains mappings from symbols to builtin executable functions
	/// </summary>
	public static Dictionary<string, Value.FunctionValue> Builtins { get; } = new()
	{
		["="] = new Value.FunctionValue( EqualityFunction ),
		["!="] = new Value.FunctionValue( NotEqualFunction ),
		[">"] = new Value.FunctionValue( GreaterThanFunction ),
		["<"] = new Value.FunctionValue( LessThanFunction ),
		[">="] = new Value.FunctionValue( GreaterThanOrEqualFunction ),
		["<="] = new Value.FunctionValue( LessThanOrEqualFunction ),
		["+"] = new Value.FunctionValue( SumFunction ),
		["-"] = new Value.FunctionValue( SubtractFunction ),
		["*"] = new Value.FunctionValue( MulFunction ),
		["set"] = new Value.FunctionValue( SetFunction ),
		["defun"] = new Value.FunctionValue( DefineFunction ),
		["pow"] = new Value.FunctionValue( PowFunction ),
		["sqrt"] = new Value.FunctionValue( SqrtFunction ),
		["if"] = new Value.FunctionValue( IfFunction ),
		["not"] = new Value.FunctionValue( NotFunction ),
		["body"] = new Value.FunctionValue( ExpressionBodyFunction ),
	};
	
	private static Value.BooleanValue GreaterThanFunction( IEnvironment env, Value[] values )
	{
		var ( a, b ) = GetTwoNumbers( env, values );
		return new Value.BooleanValue( a > b );
	}
	
	private static Value.BooleanValue LessThanFunction( IEnvironment env, Value[] values )
	{
		var ( a, b ) = GetTwoNumbers( env, values );
		return new Value.BooleanValue( a < b );
	}
	
	private static Value.BooleanValue GreaterThanOrEqualFunction( IEnvironment environment, Value[] values )
	{
		var ( a, b ) = GetTwoNumbers( environment, values );
		return new Value.BooleanValue( a >= b );
	}
	
	private static Value.BooleanValue LessThanOrEqualFunction( IEnvironment environment, Value[] values )
	{
		var ( a, b ) = GetTwoNumbers( environment, values );
		return new Value.BooleanValue( a <= b );
	}
	
	private static Value.BooleanValue EqualityFunction( IEnvironment environment, Value[] values )
	{
		if ( values.Length != 2 )
		{
			throw new InvalidParametersException( values );
		}
		
		var v1 = values[0].Evaluate( environment );
		var v2 = values[1].Evaluate( environment );
		
		if ( v1.GetType() != v2.GetType() )
		{
			return new Value.BooleanValue( false );
		}
		
		return v1 switch
		{
			Value.BooleanValue bool1 => new Value.BooleanValue( bool1.Boolean.Equals( ((Value.BooleanValue)v2).Boolean ) ),
			Value.NumberValue num1 => new Value.BooleanValue( num1.Number.Equals( ((Value.NumberValue)v2).Number ) ),
			Value.StringValue str1 => new Value.BooleanValue( str1.Text.Equals( ((Value.StringValue)v2).Text ) ),
			_ => new Value.BooleanValue( ReferenceEquals( v1, v2 ) )
		};
	}
	
	private static Value.BooleanValue NotEqualFunction( IEnvironment environment, Value[] values )
	{
		var eq = EqualityFunction( environment, values );
		return new Value.BooleanValue( !eq.Boolean );
	}
	
	private static Value ExpressionBodyFunction( IEnvironment environment, Value[] values )
	{
		if ( values.Length == 0 )
		{
			return Value.NoneValue.None;
		}
		
		Value lastValue = Value.NoneValue.None;
		
		foreach ( var expression in values )
		{
			lastValue = expression.Evaluate( environment );
		}
		
		return lastValue;
	}
	
	private static Value IfFunction( IEnvironment environment, Value[] values )
	{
		if ( values.Length is < 2 or > 3 )
		{
			throw new InvalidParametersException( values );
		}
		
		var condition = values[0].Evaluate( environment );
		
		// Check if condition is "truthy"
		var isTruthy = condition.IsTruthy();
		
		if ( isTruthy )
		{
			return values[1].Evaluate( environment );
		}
		
		return values.Length > 2 ? values[2].Evaluate( environment ) : Value.NoneValue.None;
	}
	
	private static Value.BooleanValue NotFunction( IEnvironment environment, Value[] values )
	{
		if ( values.Length != 1 )
		{
			throw new InvalidParametersException( values );
		}
		
		var value = values[0].Evaluate( environment );
		var isTruthy = value.IsTruthy();
		
		return new Value.BooleanValue( !isTruthy );
	}
	
	private static Value.NumberValue MulFunction( IEnvironment environment, Value[] values )
	{
		if ( values.Length == 0 )
		{
			return new Value.NumberValue( 1 ); // Identity for multiplication
		}
		
		var evaluatedValues = values.Select( v => v.Evaluate( environment ) ).ToArray();
		
		if ( !evaluatedValues.All( v => v is Value.NumberValue ) )
		{
			throw new InvalidParametersException( evaluatedValues );
		}
		
		var result = evaluatedValues
			.Cast<Value.NumberValue>()
			.Select( nv => nv.Number )
			.Aggregate( ( acc, v ) => acc * v );
		
		return new Value.NumberValue( result );
	}
	
	private static Value.NumberValue PowFunction( IEnvironment environment, Value[] values )
	{
		if ( values.Length != 2 )
		{
			throw new InvalidParametersException( values );
		}
		
		var baseVal = values[0].Evaluate( environment );
		var exponentVal = values[1].Evaluate( environment );
		
		if ( baseVal is not Value.NumberValue baseNum || exponentVal is not Value.NumberValue expNum )
		{
			throw new InvalidParametersException( [baseVal, exponentVal] );
		}
		
		// This can introduce precision artifacts.
		return new Value.NumberValue( new decimal( Math.Pow( (double)baseNum.Number, (double)expNum.Number ) ) );
	}
	
	private static Value.NumberValue SqrtFunction( IEnvironment environment, Value[] values )
	{
		if ( values.Length != 1 )
		{
			throw new InvalidParametersException( values );
		}
		
		var val = values[0].Evaluate( environment );
		
		if ( val is not Value.NumberValue numVal || numVal.Number < 0 )
		{
			throw new InvalidParametersException( [val] );
		}
		
		return new Value.NumberValue( new decimal( Math.Sqrt( (double)numVal.Number ) ) );
	}
	
	private static Value.FunctionValue DefineFunction( IEnvironment environment, Value[] values )
	{
		if ( values is not [Value.ListValue paramList, Value.ListValue body] )
		{
			throw new InvalidParametersException( values );
		}
		
		var argNames = paramList.ValueList.Select( p => p switch
		{
			Value.StringValue stringValue => stringValue.Text,
			Value.VariableReferenceValue variableReferenceValue => variableReferenceValue.Name,
			_ => throw new InvalidParametersException( [p] )
		} ).ToArray();
		
		return new Value.FunctionValue( ( env, arglist ) =>
		{
			if ( arglist.Length != argNames.Length )
			{
				throw new InvalidParametersException( arglist );
			}
			
			var functionEnv = new EnvironmentMap( environment );
			
			for ( var i = 0; i < argNames.Length; i++ )
			{
				functionEnv.SetVariable( argNames[i], arglist[i].Evaluate( env ) );
			}
			
			body.Deconstruct( out var valueList );
			
			return valueList.Execute( functionEnv );
		} );
	}
	
	private static Value SetFunction( IEnvironment environment, params Value[] values )
	{
		if ( values is not [Value.VariableReferenceValue vrv, _] )
		{
			throw new InvalidParametersException( values );
		}
		
		var value = values[1].Evaluate( environment );
		environment.SetVariable( vrv.Name, value );
		
		return value;
	}
	
	private static Value.NumberValue SubtractFunction( IEnvironment environment, params Value[] values )
	{
		if ( values.Length == 0 )
		{
			throw new InvalidParametersException( values );
		}
		
		var evaluatedValues = new Value[values.Length];
		
		for ( var i = 0; i < values.Length; i++ )
		{
			evaluatedValues[i] = values[i].Evaluate( environment );
			
			if ( evaluatedValues[i] is not Value.NumberValue )
			{
				throw new InvalidParametersException( evaluatedValues );
			}
		}
		
		if ( values.Length == 1 )
		{
			// Unary minus: negate the single argument
			return new Value.NumberValue( -((Value.NumberValue)evaluatedValues[0]).Number );
		}
		
		// Binary/n-ary minus: subtract all subsequent values from the first
		var result = evaluatedValues
			.Cast<Value.NumberValue>()
			.Select( nv => nv.Number )
			.Aggregate( ( acc, v ) => acc - v );
		
		return new Value.NumberValue( result );
	}
	
	private static Value SumFunction( IEnvironment environment, params Value[] values )
	{
		if ( values.Length == 0 )
		{
			return new Value.NumberValue( 0 ); // Identity for addition
		}
		
		var evaluatedValues = values.Select( v => v.Evaluate( environment ) ).ToArray();
		
		// Check if all are strings
		if ( evaluatedValues.All( v => v is Value.StringValue ) )
		{
			var result = evaluatedValues
				.Cast<Value.StringValue>()
				.Select( sv => sv.Text )
				.Aggregate( ( acc, v ) => acc + v );
			
			return new Value.StringValue( result );
		}
		
		// Check if all are numbers
		if ( evaluatedValues.All( v => v is Value.NumberValue ) )
		{
			var result = evaluatedValues.Cast<Value.NumberValue>().Select( nv => nv.Number ).Sum();
			return new Value.NumberValue( result );
		}
		
		throw new InvalidParametersException( evaluatedValues );
	}
	
	private static (decimal a, decimal b) GetTwoNumbers( IEnvironment environment, Value[] values )
	{
		if ( values.Length != 2 )
		{
			throw new InvalidParametersException( values );
		}
		
		var v1 = values[0].Evaluate( environment );
		var v2 = values[1].Evaluate( environment );
		
		if ( v1 is not Value.NumberValue n1 || v2 is not Value.NumberValue n2 )
		{
			throw new InvalidParametersException( [v1, v2] );
		}
		
		return ( n1.Number, n2.Number );
	}
}
