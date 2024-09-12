using SandLang;
using System.Collections.Generic;

namespace VNBase.Scripts;

/// <summary>
/// Defines a VNBase script.
/// </summary>
public class Script
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
	public string? Path { get; set; }

	/// <summary>
	/// If this script was initialized from a file or not.
	/// </summary>
	public bool FromFile => !string.IsNullOrEmpty( Path );

	public Script( string dialogue )
	{
		Dialogue = dialogue;
	}

	public Script( string dialogue, string path ) : this( dialogue )
	{
		Path = path;
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
