using Sandbox;
using SandLang;
using System.Collections.Generic;

namespace VNBase.Assets;

/// <summary>
/// Defines a VNBase script.
/// </summary>
public class Script : IAsset
{
	/// <summary>
	/// This is where you want to write your script.
	/// </summary>
	public virtual string Dialogue { get; set; } = string.Empty;

	/// <summary>
	/// The script to run after this one has finished.
	/// </summary>
	public virtual Script? NextScript { get; set; }

	/// <summary>
	/// If this script is initialized from a file,
	/// this is the path to that script file.
	/// </summary>
	public string Path { get; set; } = string.Empty;

	/// <summary>
	/// If this script was initialized from a file or not.
	/// </summary>
	public bool FromFile => !string.IsNullOrEmpty( Path );

	/// <summary>
	/// Create a new empty script.
	/// </summary>
	public Script() { }

	/// <summary>
	/// Create a new script from a file.
	/// </summary>
	/// <param name="path">The path to the script file.</param>
	public Script( string path )
	{
		if ( !FileSystem.Mounted.FileExists( path ) )
		{
			Log.Error( $"Unable to load script! Script file couldn't be found by path: {path}" );
			return;
		}

		Dialogue = FileSystem.Mounted.ReadAllText( path );
		Path = path;
	}

	/// <summary>
	/// Create a new script from a file.
	/// </summary>
	/// <param name="path">The path to the script file.</param>
	/// <param name="nextScript">The next script to run after this one has finished.</param>
	public Script( string path, Script nextScript ) : this( path )
	{
		NextScript = nextScript;
	}

	/// <summary>
	/// This is called when the script is run.
	/// </summary>
	public virtual void OnLoad() { }

	/// <summary>
	/// This is called after the script has finished.
	/// </summary>
	public virtual void OnUnload() { }

	/// <summary>
	/// Get this scripts local environment map.
	/// </summary>
	public virtual IEnvironment GetEnvironment()
	{
		_environment ??= new EnvironmentMap( new Dictionary<string, Value>()
		{
			// You may manually assign unique script variables here.
		} );

		return _environment;
	}

	private IEnvironment? _environment;
}
