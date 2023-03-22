using Sandbox;

namespace VNBase;

/// <summary>
/// Defines a script that can be run.
/// </summary>
public class ScriptBase : BaseNetworkable
{
	/// <summary>
	/// The path to the background image to display.
	/// If none, we assume the camera background.
	/// </summary>
	public string BgImage { get; set; }

	/// <summary>
	/// This is where you want to write your script.
	/// </summary>
	public virtual string Dialogue { get; set; }

	/// <summary>
	/// The script to run after this one.
	/// </summary>
	public virtual ScriptBase NextScript { get; set; }

	/// <summary>
	/// This is called when the script is run.
	/// </summary>
	public virtual void Before()
	{

	}

	/// <summary>
	/// This is called after the script has finished.
	/// </summary>
	public virtual void After()
	{

	}
}
