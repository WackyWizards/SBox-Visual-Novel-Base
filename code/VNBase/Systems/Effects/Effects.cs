using System;
using System.Threading.Tasks;

namespace VNBase;

/// <summary>
/// This class contains all the effects that can be used in the VNBase.
/// </summary>
public class Effects
{
	/// <summary>
	/// A simple typewriter effect.
	/// </summary>
	public class Typewriter
	{
		public static async Task<bool> Play( string text, int delay, Action<string> callback )
		{
			string newText = "";
			var tcs = new TaskCompletionSource<bool>();

			for ( int i = 0; i < text.Length; i++ )
			{
				newText += text[i];
				callback( newText );
				await Task.Delay( delay );
			}

			tcs.SetResult( true );
			return tcs.Task.Result;
		}
	}
}
