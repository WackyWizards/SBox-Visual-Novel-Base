
namespace VNBase;

/// <summary>
/// Defines a VNBase script.
/// </summary>
public class Script
{
	/// <summary>
	/// This is where you want to write your script.
	/// </summary>
	public virtual string Dialogue { get; set; }

	/// <summary>
	/// The script to run after this one.
	/// </summary>
	public virtual Script NextScript { get; set; }

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
}
