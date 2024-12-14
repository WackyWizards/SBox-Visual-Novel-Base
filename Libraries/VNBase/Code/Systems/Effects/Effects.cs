using System;
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

	/// <summary>
	/// A simple typewriter effect.
	/// </summary>
	public class Typewriter : ITextEffect
	{
		public async Task<bool> Play( string text, int delay, Action<string> callback, CancellationToken cancellationToken )
		{
			var newText = "";

			foreach (var character in text)
			{
				if ( cancellationToken.IsCancellationRequested )
				{
					return false;
				}

				newText += character;
				callback( newText );
				await Task.Delay( delay, cancellationToken );
			}

			return true;
		}
	}
}
