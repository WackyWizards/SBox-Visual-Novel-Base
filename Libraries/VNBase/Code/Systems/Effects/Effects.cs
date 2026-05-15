using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VNBase;

/// <summary>
/// Contains all the base effects that can be used.
/// </summary>
public static class Effects
{
	public interface ITextEffect
	{
		public Task<bool> Play( string text, int delay, Action<string> callback, CancellationToken cancellationToken );
	}
	
	public class Typewriter : ITextEffect
	{
		public async Task<bool> Play( string text, int delay, Action<string> callback, CancellationToken cancellationToken )
		{
			var newText = new StringBuilder();
			var i = 0;
			
			while ( i < text.Length )
			{
				if ( cancellationToken.IsCancellationRequested )
				{
					return false;
				}
				
				var chunkEnd = FindChunkEnd( text, i );
				
				if ( chunkEnd != -1 )
				{
					newText.Append( text, i, chunkEnd - i + 1 );
					i = chunkEnd + 1;
				}
				else
				{
					newText.Append( text[i] );
					i++;
					await Task.Delay( delay, cancellationToken );
				}
				
				callback( newText.ToString() );
			}
			
			return true;
		}
		
		/// <summary>
		/// If the character at <paramref name="start"/> begins an HTML tag or entity,
		/// returns the index of its closing character. Otherwise, returns -1.
		/// </summary>
		private static int FindChunkEnd( string text, int start )
		{
			return text[start] switch
			{
				'<' => FindTagEnd( text, start ), '&' => FindEntityEnd( text, start ), _ => -1
			};
		}
		
		/// <summary>
		/// Finds the closing '>' of an HTML tag, correctly skipping over quoted attribute values.
		/// </summary>
		private static int FindTagEnd( string text, int start )
		{
			var i = start + 1;
			
			while ( i < text.Length )
			{
				switch ( text[i] )
				{
					case '"' or '\'':
					{
						// Skip quoted attribute value
						var quote = text[i++];
						while ( i < text.Length && text[i] != quote ) i++;
						
						break;
					}
					case '>':
					{
						return i;
					}
				}
				
				i++;
			}
			
			return -1;
		}
		
		/// <summary>
		/// Finds the closing ';' of an HTML entity, validating it is actually a well-formed entity.
		/// Returns -1 for stray '&' characters that aren't valid entities.
		/// </summary>
		private static int FindEntityEnd( string text, int start )
		{
			var semicolonIndex = text.IndexOf( ';', start + 1 );
			
			if ( semicolonIndex == -1 )
			{
				return -1;
			}
			
			var entity = text.Substring( start + 1, semicolonIndex - start - 1 );
			return IsValidEntity( entity ) ? semicolonIndex : -1;
		}
		
		/// <summary>
		/// Returns true if the string is a valid HTML entity name or numeric reference.
		/// e.g. "lt", "amp", "#169", "#x1F600"
		/// </summary>
		private static bool IsValidEntity( string entity )
		{
			if ( string.IsNullOrEmpty( entity ) )
			{
				return false;
			}
			
			if ( entity[0] != '#' )
			{
				return entity.All( char.IsLetter );
			}
			
			var rest = entity[1..];
			
			return rest.StartsWith( 'x' ) || rest.StartsWith( 'X' )
				? rest.Length > 1 && rest[1..].All( IsHexDigit )
				: rest.Length > 0 && rest.All( char.IsDigit );
		}
		
		private static bool IsHexDigit( char c ) => char.IsDigit( c ) || c is >= 'a' and <= 'f' or >= 'A' and <= 'F';
	}
}
