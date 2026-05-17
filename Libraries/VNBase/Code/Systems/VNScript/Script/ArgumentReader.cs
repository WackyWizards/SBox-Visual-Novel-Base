using System;

namespace VNScript;

public partial class Script
{
	/// <summary>
	/// Reads arguments from a <see cref="SParen"/>
	/// </summary>
	private sealed class ArgumentReader( SParen arguments, int startIndex = 0 )
	{
		private int _index = startIndex;
		
		public bool HasMore => _index < arguments.Count;
		
		public Value Peek()
		{
			if ( !HasMore )
			{
				throw new InvalidOperationException( "No more arguments to read." );
			}
			
			return arguments[_index];
		}
		
		public Value Read()
		{
			if ( !HasMore )
			{
				throw new InvalidOperationException( "No more arguments to read." );
			}
			
			return arguments[_index++];
		}
		
		public T Read<T>() where T : Value
		{
			var value = Read();
			if ( value is not T typed )
			{
				throw new InvalidParametersException( [value] );
			}
			
			return typed;
		}
		
		public T ReadAs<T>() where T : Value
		{
			return Read<T>();
		}
	}
}
