using SandLang;
using System.Collections.Generic;

namespace VNBase;

/// <summary>
/// Defines a VNBase script.
/// </summary>
public class Script
{
	/// <summary>
	/// This is where you want to write your script dialogue.
	/// </summary>
	public virtual string Dialogue { get; set; } = string.Empty;

	/// <summary>
	/// The script to run after this one has finished.
	/// </summary>
	public virtual Script? NextScript { get; set; }

	/// <summary>
	/// This is called when the script is run.
	/// </summary>
	public virtual void OnLoad()
	{

	}

	/// <summary>
	/// This is called after the script has finished.
	/// </summary>
	public virtual void After()
	{

	}

	/// <summary>
	/// Get this scripts local environment map.
	/// </summary>
	public virtual IEnvironment GetEnvironment()
	{
		return new EnvironmentMap( new Dictionary<string, Value>()
		{
			// Assign unique script variables here.
		} );
	}
}
