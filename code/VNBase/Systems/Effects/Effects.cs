using System;
using System.Threading;
using System.Threading.Tasks;

namespace VNBase;

/// <summary>
/// This class contains all the base effects that can be used in the VNBase.
/// </summary>
public class Effects
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
			string newText = "";
			var tcs = new TaskCompletionSource<bool>();

			for ( int i = 0; i < text.Length; i++ )
			{
				if ( cancellationToken.IsCancellationRequested )
				{
					tcs.SetCanceled( cancellationToken );
					return tcs.Task.Result;
				}

				newText += text[i];
				callback( newText );
				await Task.Delay( delay, cancellationToken );
			}

			tcs.SetResult( true );
			return tcs.Task.Result;
		}
	}
}
